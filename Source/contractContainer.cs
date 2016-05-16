#region license
/*The MIT License (MIT)
Contract Container - An object for storing information about contracts

Copyright (c) 2016 DMagic

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using UnityEngine;
using FinePrint.Utilities;
using Contracts;
using Contracts.Templates;
using Contracts.Agents;
using FinePrint.Contracts;
using FinePrint.Contracts.Parameters;

namespace ContractParser
{
	public class contractContainer
	{
		private Contract root;
		private Guid id;
		private float totalFundsReward, totalRepReward, totalSciReward;
		private float totalFundsPenalty, totalRepPenalty;
		private double expire, duration, deadline, completed;
		private bool showNote, canBeDeclined, canBeCancelled, initialized;
		private string briefing;
		private string daysToExpire;
		private string targetPlanet;
		private string title;
		private string notes;
		private float fundsRew, fundsPen, fundsAdv, repRew, repPen, sciRew, decPen;
		private float fundsRewStrat, fundsPenStrat, fundsAdvStrat, repRewStrat, repPenStrat, sciRewStrat;
		private string fundsAdvString, fundsRewString, fundsPenString, repRewString, repPenString, sciRewString, decPenString;
		private Agent agent;
		private List<parameterContainer> paramList = new List<parameterContainer>();
		private List<parameterContainer> allParamList = new List<parameterContainer>();

		private static KSPUtil.DefaultDateTimeFormatter timeFormatter;

		public contractContainer(Contract c)
		{
			root = c;

			try
			{
				id = root.ContractGuid;
			}
			catch (Exception e)
			{
				Debug.LogError("[Contract Parser] Contract Guid not set, skipping...\n" + e);
				root = null;
				return;
			}

			try
			{
				title = root.Title;
			}
			catch (Exception e)
			{
				Debug.LogError("[Contract Parser] Contract Title not set, using type name...\n" + e);
				title = root.GetType().Name;
			}

			try
			{
				notes = root.Notes;
			}
			catch (Exception e)
			{
				Debug.LogError("[Contract Parser] Contract Notes not set, blank notes used...\n" + e);
				notes = "";
			}

			try
			{
				briefing = root.Description;
			}
			catch (Exception e)
			{
				Debug.LogError("[Contract Parser] Contract Briefing not set, blank briefing used...\n" + e);
				briefing = "";
			}

			try
			{
				canBeDeclined = root.CanBeDeclined();
			}
			catch (Exception e)
			{
				Debug.LogError("[Contract Parser] Contract Decline state not set, using true...\n" + e);
				canBeDeclined = true;
			}

			try
			{
				canBeCancelled = root.CanBeCancelled();
			}
			catch (Exception e)
			{
				Debug.LogError("[Contract Parser] Contract Cancel state not set, using true...\n" + e);
				canBeCancelled = true;
			}

			if (root.Agent != null)
				agent = root.Agent;
			else
				agent = AgentList.Instance.GetAgentRandom();

			if (c.DateDeadline <= 0)
			{
				duration = double.MaxValue;
				daysToExpire = "----";
			}
			else
			{
				duration = root.DateDeadline - Planetarium.GetUniversalTime();
				//Calculate time in day values using Kerbin or Earth days
				daysToExpire = timeInDays(duration);
			}

			updateTimeValues();

			contractRewards();
			contractPenalties();
			contractAdvance();

			decPen = HighLogic.CurrentGame.Parameters.Career.RepLossDeclined;
			decPenString = decPen.ToString("F0");

			totalFundsReward = rewards();
			totalFundsPenalty = penalties();
			totalRepReward = repRewards();
			totalSciReward = sciRewards();
			totalRepPenalty = repPenalties();

			//Generate four layers of parameters
			for (int i = 0; i < c.ParameterCount; i++)
			{
				ContractParameter param = c.GetParameter(i);

				if (param == null)
					continue;

				addContractParam(param, 0);
			}

			CelestialBody t = getTargetBody();

			targetPlanet = t == null ? "" : t.name;
		}

		private void addContractParam(ContractParameter param, int Level)
		{
			parameterContainer cc = new parameterContainer(this, param, Level);
			paramList.Add(cc);
			allParamList.Add(cc);
		}

		private void contractRewards()
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractReward, (float)root.FundsCompletion, root.ScienceCompletion, root.ReputationCompletion);

			fundsRew = (float)root.FundsCompletion;
			fundsRewStrat = currencyQuery.GetEffectDelta(Currency.Funds);

			if (fundsRewStrat != 0)
				fundsRewString = string.Format("+ {0:N0} ({1:N0})", fundsRew + fundsRewStrat, fundsRewStrat);
			else if (fundsRew != 0)
				fundsRewString = "+ " + fundsRew.ToString("N0");
			else
				fundsRewString = "";

			repRew = root.ReputationCompletion;
			repRewStrat = currencyQuery.GetEffectDelta(Currency.Reputation);

			if (repRewStrat != 0)
				repRewString = string.Format("+ {0:N0} ({1:N0})", repRew + repRewStrat, repRewStrat);
			else if (repRew != 0)
				repRewString = "+ " + repRew.ToString("N0");
			else
				repRewString = "";

			sciRew = root.ScienceCompletion;
			sciRewStrat = currencyQuery.GetEffectDelta(Currency.Science);

			if (sciRewStrat != 0)
				sciRewString = string.Format("+ {0:N0} ({1:N0})", sciRew + sciRewStrat, sciRewStrat);
			else if (sciRew != 0)
				sciRewString = "+ " + sciRew.ToString("N0");
			else
				sciRewString = "";
		}

		private void contractPenalties()
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractPenalty, (float)root.FundsFailure, 0f, root.ReputationFailure);

			fundsPen = (float)root.FundsFailure;
			fundsPenStrat = currencyQuery.GetEffectDelta(Currency.Funds);

			if (fundsPenStrat != 0)
				fundsPenString = string.Format("- {0:N0} ({1:N0})", fundsPen + fundsPenStrat, fundsPenStrat);
			else if (fundsPen != 0)
				fundsPenString = "- " + fundsPen.ToString("N0");
			else
				fundsPenString = "";

			repPen = root.ReputationFailure;
			repPenStrat = currencyQuery.GetEffectDelta(Currency.Reputation);

			if (repPenStrat != 0)
				repPenString = string.Format("- {0:N0} ({1:N0})", repPen + repPenStrat, repPenStrat);
			else if (repPen != 0)
				repPenString = "- " + repPen.ToString("N0");
			else
				repPenString = "";
		}

		private void contractAdvance()
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractAdvance, (float)root.FundsAdvance, 0, 0);

			fundsAdv = (float)root.FundsAdvance;
			fundsAdvStrat = currencyQuery.GetEffectDelta(Currency.Funds);

			if (fundsAdvStrat != 0)
				fundsAdvString = string.Format("- {0:N0} ({1:N0})", fundsAdv + fundsAdvStrat, fundsAdvStrat);
			else if (fundsAdv != 0)
				fundsAdvString = fundsAdv.ToString("N0");
			else
				fundsAdvString = "";
		}

		private float rewards()
		{
			float f = 0;
			f += fundsRew + fundsRewStrat;
			f += fundsAdv + fundsAdvStrat;
			foreach (parameterContainer p in allParamList)
				f += p.FundsRew + p.FundsRewStrat;
			return f;
		}

		private float penalties()
		{
			float f = 0;
			f += fundsPen + fundsPenStrat;
			foreach (parameterContainer p in allParamList)
				f += p.FundsPen + p.FundsPenStrat;
			return f;
		}

		private float repRewards()
		{
			float f = 0;
			f += repRew + repRewStrat;
			foreach (parameterContainer p in allParamList)
				f += p.RepRew + p.RepRewStrat;
			return f;
		}

		private float repPenalties()
		{
			float f = 0;
			f += repPen + repPenStrat;
			foreach (parameterContainer p in allParamList)
				f += p.RepPen + p.RepPenStrat;
			return f;
		}

		private float sciRewards()
		{
			float f = 0;
			f += sciRew + sciRewStrat;
			foreach (parameterContainer p in allParamList)
				f += p.SciRew + p.SciRewStrat;
			return f;
		}

		public void updateContractInfo()
		{
			contractRewards();
			contractPenalties();
			contractAdvance();
		}

		public void updateFullParamInfo()
		{
			//Clear out all existing parameters and regenerate new ones
			paramList.Clear();
			allParamList.Clear();

			for (int i = 0; i < root.ParameterCount; i++)
			{
				ContractParameter param = root.GetParameter(i);

				if (param == null)
					continue;

				addContractParam(param, 0);
			}
		}

		public void updateParameterInfo()
		{
			foreach (parameterContainer pC in allParamList)
			{
				pC.paramRewards();
				pC.paramPenalties();
			}
		}

		public void updateParameterInfo(Type t)
		{
			foreach (parameterContainer pC in allParamList)
			{
				if (pC.CParam.GetType() == t)
				{
					pC.paramRewards();
					pC.paramPenalties();
				}
			}
		}

		public void addToParamList(parameterContainer pC)
		{
			allParamList.Add(pC);
		}

		public parameterContainer getParameter(int i)
		{
			if (paramList.Count > i)
				return paramList[i];

			return null;
		}

		private CelestialBody getTargetBody()
		{
			if (root == null)
				return null;

			bool checkTitle = false;

			Type t = root.GetType();

			try
			{
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
					var fields = typeof(WorldFirstContract).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

					var milestone = fields[0].GetValue((WorldFirstContract)root) as ProgressMilestone;

					if (milestone == null)
						return null;

					return milestone.body;
				}
				else
					checkTitle = true;
			}
			catch (Exception e)
			{
				Debug.LogError("[Contract Parser] Error Detecting Target Celestial Body...\n" + e);
				return null;
			}

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

		public string timeInDays(double D)
		{
			if (D <= 0)
				return "----";

			if (timeFormatter == null)
				timeFormatter = new KSPUtil.DefaultDateTimeFormatter();

			int[] time = timeFormatter.GetDateFromUT((int)D);
			StringBuilder s = new StringBuilder();

			if (time[4] > 0)
				s.Append(string.Format("{0}y", time[4]));
			if (time[3] > 0)
			{
				if (!string.IsNullOrEmpty(s.ToString()))
					s.Append(" ");
				s.Append(string.Format("{0}d", time[3]));
			}
			if (time[4] <= 0 && time[2] > 0)
			{
				if (!string.IsNullOrEmpty(s.ToString()))
					s.Append(" ");
				s.Append(string.Format("{0}h", time[2]));
			}
			if (time[4] <= 0 && time[3] <= 0 && time[2] <= 0 && time[1] > 0)
				s.Append(string.Format("{0}m", time[1]));

			return s.ToString();
		}

		public Contract Root
		{
			get { return root; }
		}

		public Agent RootAgent
		{
			get { return agent; }
		}

		public string Briefing
		{
			get { return briefing; }
		}

		public Guid ID
		{
			get { return id; }
		}

		public int ParameterCount
		{
			get { return allParamList.Count; }
		}

		public int FirstLevelParameterCount
		{
			get { return paramList.Count; }
		}

		public parameterContainer getParameterFull(int index)
		{
			if (allParamList.Count > index)
				return allParamList[index];

			return null;
		}

		public parameterContainer getParameterLevelOne(int index)
		{
			if (paramList.Count > index)
				return paramList[index];

			return null;
		}

		public double Duration
		{
			get { return duration; }
			set { duration = value; }
		}

		public bool Initialized
		{
			get { return initialized; }
			set { initialized = value; }
		}

		public bool ShowNote
		{
			get { return showNote; }
			set { showNote = value; }
		}

		public string DaysToExpire
		{
			get { return daysToExpire; }
			set { daysToExpire = value; }
		}

		public string Title
		{
			get { return title; }
			set { title = value; }
		}

		public string Notes
		{
			get { return notes; }
			set { notes = value; }
		}

		public bool CanBeDeclined
		{
			get { return canBeDeclined; }
		}

		public bool CanBeCancelled
		{
			get { return canBeCancelled; }
		}

		public float DecPen
		{
			get { return decPen; }
		}

		public string DecPenString
		{
			get { return decPenString; }
		}

		public string TargetPlanet
		{
			get { return targetPlanet; }
		}

		public float FundsAdv
		{
			get { return fundsAdv; }
		}

		public float FundsRew
		{
			get { return fundsRew; }
		}

		public float FundsPen
		{
			get { return fundsPen; }
		}

		public float RepRew
		{
			get { return repRew; }
		}

		public float RepPen
		{
			get { return repPen; }
		}

		public float SciRew
		{
			get { return sciRew; }
		}

		public float FundsAdvStrat
		{
			get { return fundsAdvStrat; }
		}

		public float FundsRewStrat
		{
			get { return fundsRewStrat; }
		}

		public float FundsPenStrat
		{
			get { return fundsPenStrat; }
		}

		public float RepRewStrat
		{
			get { return repRewStrat; }
		}

		public float RepPenStrat
		{
			get { return repPenStrat; }
		}

		public float SciRewStrat
		{
			get { return sciRewStrat; }
		}

		public string FundsRewString
		{
			get { return fundsRewString; }
		}

		public string FundsPenString
		{
			get { return fundsPenString; }
		}

		public string FundsAdvString
		{
			get { return fundsAdvString; }
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

		public float TotalReward
		{
			get { return totalFundsReward; }
		}

		public float TotalPenalty
		{
			get { return totalFundsPenalty; }
		}

		public float TotalRepReward
		{
			get { return totalRepReward; }
		}

		public float TotalRepPenalty
		{
			get { return totalRepPenalty; }
		}

		public float TotalSciReward
		{
			get { return totalSciReward; }
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

		public double Completed
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
