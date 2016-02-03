using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using UnityEngine;
using FinePrint.Utilities;
using Contracts;
using Contracts.Templates;
using FinePrint.Contracts;
using FinePrint.Contracts.Parameters;

namespace ContractParser
{
	public class contractContainer
	{
		private Contract root;
		private Guid id;
		private double totalReward, duration;
		private double expire, deadline, completed;
		private bool showNote;
		private string daysToExpire;
		private string targetPlanet;
		private string title = "";
		private string notes = "";
		private string fundsRewString, fundsPenString, repRewString, repPenString, sciRewString;
		private List<parameterContainer> paramList = new List<parameterContainer>();
		private List<parameterContainer> allParamList = new List<parameterContainer>();

		public contractContainer(Contract c)
		{
			root = c;

			try
			{
				id = root.ContractGuid;
			}
			catch (Exception e)
			{
				Debug.LogError("[CapCom] Contract Guid not set, skipping...: " + e);
				root = null;
				return;
			}

			showNote = false;
			title = c.Title;
			notes = c.Notes;

			if (c.DateDeadline <= 0)
			{
				duration = double.MaxValue;
				daysToExpire = "----";
			}
			else
			{
				duration = root.DateDeadline - Planetarium.GetUniversalTime();
				//Calculate time in day values using Kerbin or Earth days
				//daysToExpire = contractScenario.timeInDays(duration);
			}

			contractRewards(c);
			contractPenalties(c);

			totalReward = c.FundsCompletion;
			foreach (ContractParameter param in c.AllParameters)
				totalReward += param.FundsCompletion;

			//Generate four layers of parameters, check if each is an altitude parameter
			for (int i = 0; i < c.ParameterCount; i++)
			{
				ContractParameter param = c.GetParameter(i);
				addContractParam(param, 0);
			}

			CelestialBody t = getTargetBody();

			targetPlanet = t == null ? "" : t.name;
		}

		private void addContractParam(ContractParameter param, int Level)
		{
			//string partTest = contractScenario.paramTypeCheck(param);
			paramList.Add(new parameterContainer(this, param, Level, ""));
			allParamList.Add(paramList.Last());
		}

		private void contractRewards(Contract c)
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractReward, (float)c.FundsCompletion, c.ScienceCompletion, c.ReputationCompletion);

			fundsRewString = "";
			if (c.FundsCompletion != 0)
				fundsRewString = "+ " + c.FundsCompletion.ToString("N0");
			float fundsRewStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if (fundsRewStrat != 0f)
				fundsRewString = string.Format("+ {0:N0} ({1:N0})", c.FundsCompletion + fundsRewStrat, fundsRewStrat);

			repRewString = "";
			if (c.ReputationCompletion != 0)
				repRewString = "+ " + c.ReputationCompletion.ToString("N0");
			float repRewStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if (repRewStrat != 0f)
				repRewString = string.Format("+ {0:N0} ({1:N0})", c.ReputationCompletion + repRewStrat, repRewStrat);

			sciRewString = "";
			if (c.ScienceCompletion != 0)
				sciRewString = "+ " + c.ScienceCompletion.ToString("N0");
			float sciRewStrat = currencyQuery.GetEffectDelta(Currency.Science);
			if (sciRewStrat != 0f)
			{
				sciRewString = string.Format("+ {0:N0} ({1:N0})", c.ScienceCompletion + sciRewStrat, sciRewStrat);
			}
		}

		private void contractPenalties(Contract c)
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractPenalty, (float)c.FundsFailure, 0f, c.ReputationFailure);

			fundsPenString = "";
			if (c.FundsFailure != 0)
				fundsPenString = "- " + c.FundsFailure.ToString("N0");
			float fundsPenStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if (fundsPenStrat != 0f)
			{
				fundsPenString = string.Format("- {0:N0} ({1:N0})", c.FundsFailure + fundsPenStrat, fundsPenStrat);
			}

			repPenString = "";
			if (c.ReputationFailure != 0)
				repPenString = "- " + c.ReputationFailure.ToString("N0");
			float repPenStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if (repPenStrat != 0f)
			{
				repPenString = string.Format("- {0:N0} ({1:N0})", c.ReputationFailure + repPenStrat, repPenStrat);
			}
		}

		internal void updateContractInfo()
		{
			contractRewards(root);
			contractPenalties(root);
		}

		internal void updateFullParamInfo()
		{
			totalReward = root.FundsCompletion;
			foreach (ContractParameter param in root.AllParameters)
				totalReward += param.FundsCompletion;

			//Clear out all existing parameters and regenerate new ones

			paramList.Clear();
			allParamList.Clear();

			for (int i = 0; i < root.ParameterCount; i++)
			{
				ContractParameter param = root.GetParameter(i);
				addContractParam(param, 0);
			}
		}

		internal void updateParameterInfo()
		{
			foreach (parameterContainer pC in allParamList)
			{
				pC.paramRewards(pC.CParam);
				pC.paramPenalties(pC.CParam);
			}
		}

		internal void updateParameterInfo(Type t)
		{
			foreach (parameterContainer pC in allParamList)
			{
				if (pC.CParam.GetType() == t)
				{
					pC.paramRewards(pC.CParam);
					pC.paramPenalties(pC.CParam);
				}
			}
		}

		internal void addToParamList(parameterContainer pC)
		{
			allParamList.Add(pC);
		}

		private CelestialBody getTargetBody()
		{
			if (root == null)
				return null;

			bool checkTitle = false;

			Type t = root.GetType();

			if (t == typeof(CollectScience))
				return ((CollectScience)root).TargetBody;
			else if (t == typeof(ExploreBody))
				return ((ExploreBody)root).TargetBody;
			else if (t == typeof(PartTest))
			{
				var fields = typeof(PartTest).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				return fields[1].GetValue((PartTest)root) as CelestialBody;
			}
			else if (t == typeof(PlantFlag))
				return ((PlantFlag)root).TargetBody;
			else if (t == typeof(RecoverAsset))
			{
				var fields = typeof(RecoverAsset).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				return fields[0].GetValue((RecoverAsset)root) as CelestialBody;
			}
			else if (t == typeof(GrandTour))
				return ((GrandTour)root).TargetBodies.LastOrDefault();
			else if (t == typeof(ARMContract))
			{
				var fields = typeof(ARMContract).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				return fields[0].GetValue((ARMContract)root) as CelestialBody;
			}
			else if (t == typeof(BaseContract))
				return ((BaseContract)root).targetBody;
			else if (t == typeof(ISRUContract))
				return ((ISRUContract)root).targetBody;
			else if (t == typeof(SatelliteContract))
			{
				SpecificOrbitParameter p = root.GetParameter<SpecificOrbitParameter>();

				if (p == null)
					return null;

				return p.TargetBody;
			}
			else if (t == typeof(StationContract))
				return ((StationContract)root).targetBody;
			else if (t == typeof(SurveyContract))
				return ((SurveyContract)root).targetBody;
			else if (t == typeof(TourismContract))
				return null;
			else if (t == typeof(WorldFirstContract))
			{
				ProgressTrackingParameter p = root.GetParameter<ProgressTrackingParameter>();

				if (p == null)
					return null;

				var fields = typeof(ProgressTrackingParameter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				var milestone = fields[0].GetValue(p) as ProgressMilestone;

				if (milestone == null)
					return null;

				return milestone.body;
			}
			else
				checkTitle = true;

			if (checkTitle)
			{
				foreach (CelestialBody b in FlightGlobals.Bodies)
				{
					string n = b.name;

					Regex r = new Regex(string.Format(@"\b{0}\b", n));

					if (r.IsMatch(title))
						return b;
				}
			}

			return null;
		}

		public void updateTimeValues()
		{
			expire = root.DateExpire;
			if (expire <= 0)
				expire = double.MaxValue;

			deadline = root.DateDeadline;
			if (deadline <= 0)
				deadline = double.MaxValue;

			completed = root.DateFinished;
		}

		public Contract Contract
		{
			get { return root; }
		}

		public Guid ID
		{
			get { return id; }
		}

		public int ParameterCount
		{
			get { return allParamList.Count; }
		}

		public double TotalReward
		{
			get { return totalReward; }
		}

		public double Duration
		{
			get { return duration; }
			internal set { duration = value; }
		}

		public bool ShowNote
		{
			get { return showNote; }
			internal set { showNote = value; }
		}

		public string DaysToExpire
		{
			get { return daysToExpire; }
			internal set { daysToExpire = value; }
		}

		public string Title
		{
			get { return title; }
			internal set { title = value; }
		}

		public string Notes
		{
			get { return notes; }
			internal set { notes = value; }
		}

		public string TargetPlanet
		{
			get { return targetPlanet; }
		}

		public string FundsRewString
		{
			get { return fundsRewString; }
		}

		public string FundsPenString
		{
			get { return fundsPenString; }
		}

		public string RepRewString
		{
			get { return repRewString; }
		}

		public string RepPenString
		{
			get { return repPenString; }
		}

		public string SciRewString
		{
			get { return sciRewString; }
		}

		public double Expire
		{
			get { return expire; }
		}

		public double Deadline
		{
			get { return deadline; }
		}

		public double Finished
		{
			get { return completed; }
		}

		public List<parameterContainer> ParamList
		{
			get { return paramList; }
		}

		public List<parameterContainer> AllParamList
		{
			get { return allParamList; }
		}


	}
}
