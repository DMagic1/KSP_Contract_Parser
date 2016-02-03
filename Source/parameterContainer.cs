using System;
using System.Collections.Generic;
using System.Linq;
using Contracts;
using FinePrint.Contracts;
using FinePrint.Contracts.Parameters;

namespace ContractParser
{
	public class parameterContainer
	{
		private contractContainer root;
		private ContractParameter cParam;
		private bool showNote;
		private string title = "";
		private string notes = "";
		private string customNotes = "";
		private int level;
		private string fundsRewString, fundsPenString, repRewString, repPenString, sciRewString;
		private AvailablePart part;
		private List<parameterContainer> paramList = new List<parameterContainer>();

		public parameterContainer(contractContainer Root, ContractParameter cP, int Level, string PartTestName)
		{
			root = Root;
			cParam = cP;
			showNote = false;
			level = Level;
			paramRewards(cP);
			paramPenalties(cP);
			title = cParam.Title;
			notes = cParam.Notes;

			if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedScene == GameScenes.SPACECENTER)
				customNotes = setCustomNotes();

			if (level < 4)
			{
				for (int i = 0; i < cParam.ParameterCount; i++)
				{
					ContractParameter param = cParam.GetParameter(i);
					addSubParam(param, level + 1);
				}
			}

			//if (!string.IsNullOrEmpty(PartTestName))
			//{
			//	if (PartTestName == "partTest")
			//	{
			//		part = ((Contracts.Parameters.PartTest)cParam).tgtPartInfo;
			//		DMC_MBE.LogFormatted_DebugOnly("Part Assigned For Stock Part Test");
			//	}
			//	else if (PartTestName == "MCEScience")
			//	{
			//		if (contractAssembly.MCELoaded)
			//		{
			//			part = PartLoader.Instance.parts.FirstOrDefault(p => p.partPrefab.partInfo.title == contractAssembly.MCEPartName(cParam));
			//			if (part != null)
			//				DMC_MBE.LogFormatted_DebugOnly("Part Assigned For Mission Controller Contract");
			//			else
			//				DMC_MBE.LogFormatted_DebugOnly("Part Not Found");
			//		}
			//	}
			//	else if (PartTestName == "DMcollectScience")
			//	{
			//		if (contractAssembly.DMLoaded)
			//		{
			//			part = PartLoader.getPartInfoByName(contractAssembly.DMagicSciencePartName(cParam));
			//			if (part != null)
			//				DMC_MBE.LogFormatted_DebugOnly("Part Assigned For DMagic Contract");
			//			else
			//				DMC_MBE.LogFormatted_DebugOnly("Part Not Found");
			//		}
			//	}
			//	else if (PartTestName == "DManomalyScience")
			//	{
			//		if (contractAssembly.DMALoaded)
			//		{
			//			part = PartLoader.getPartInfoByName(contractAssembly.DMagicAnomalySciencePartName(cParam));
			//			if (part != null)
			//				DMC_MBE.LogFormatted_DebugOnly("Part Assigned For DMagic Anomaly Contract");
			//			else
			//				DMC_MBE.LogFormatted_DebugOnly("Part Not Found");
			//		}
			//	}
			//	else if (PartTestName == "DMasteroidScience")
			//	{
			//		if (contractAssembly.DMAstLoaded)
			//		{
			//			part = PartLoader.getPartInfoByName(contractAssembly.DMagicAsteroidSciencePartName(cParam));
			//			if (part != null)
			//				DMC_MBE.LogFormatted_DebugOnly("Part Assigned For DMagic Asteroid Contract");
			//			else
			//				DMC_MBE.LogFormatted_DebugOnly("Part Not Found");
			//		}
			//	}
			//	else
			//		part = null;
			//}
		}

		private string setCustomNotes()
		{
			string s = "";
			//Type pType = cParam.GetType();

			//if (pType == typeof(PartRequestParameter))
			//{
			//	List<string> l = new List<string>();

			//	if (contractAssembly.FPPartLoaded)
			//	{
			//		l = contractAssembly.FPPartRequestList((PartRequestParameter)cParam);

			//		if (l.Count > 0)
			//		{
			//			List<string> titles = getPartTitles(l);

			//			if (titles.Count > 0)
			//			{
			//				if (!string.IsNullOrEmpty(notes))
			//					s = "\n";

			//				s += "The following parts are acceptable:";

			//				for (int i = 0; i < titles.Count; i++)
			//				{
			//					string t = titles[i];

			//					s += "\n" + t;
			//				}
			//			}
			//		}
			//	}

			//	if (contractAssembly.FPModLoaded)
			//	{
			//		l = contractAssembly.FPModuleRequestList((PartRequestParameter)cParam);

			//		if (l.Count > 0)
			//		{
			//			List<string> titles = getPartTitlesFromModules(l);

			//			if (titles.Count > 0)
			//			{
			//				if (string.IsNullOrEmpty(s))
			//				{
			//					if (!string.IsNullOrEmpty(notes))
			//						s = "\n";

			//					s = "The following parts are acceptable:";
			//				}

			//				for (int i = 0; i < titles.Count; i++)
			//				{
			//					string t = titles[i];

			//					s += "\n" + t;
			//				}
			//			}
			//		}
			//	}
			//}
			//else if (pType == typeof(VesselSystemsParameter))
			//{
			//	List<string> l = new List<string>();

			//	if (contractAssembly.FPVesselSystemsLoaded)
			//	{
			//		l = contractAssembly.FPVesselSystemsList((VesselSystemsParameter)cParam);

			//		if (l.Count > 0)
			//		{

			//			for (int j = 0; j < l.Count; j++)
			//			{
			//				List<string> modNames = FinePrint.ContractDefs.GetModules(l[j]);

			//				List<string> titles = getPartTitlesFromModules(modNames);

			//				if (titles.Count > 0)
			//				{
			//					s += string.Format("\nThe following parts are acceptable for Module Type - {0}:", l[j]);

			//					for (int i = 0; i < titles.Count; i++)
			//					{
			//						string t = titles[i];

			//						s += "\n" + t;
			//					}
			//				}
			//			}
			//		}
			//	}
			//}

			return s;
		}

		private List<string> getPartTitles(List<string> names)
		{
			List<string> l = new List<string>();

			for (int i = 0; i < names.Count; i++)
			{
				string s = names[i];

				if (string.IsNullOrEmpty(s))
					continue;

				AvailablePart p = PartLoader.getPartInfoByName(s);

				if (p == null)
					continue;

				if (!ResearchAndDevelopment.PartModelPurchased(p))
					continue;

				l.Add(p.title);
			}

			return l;
		}

		private List<string> getPartTitlesFromModules(List<string> names)
		{
			List<string> l = new List<string>();

			for (int i = 0; i < names.Count; i++)
			{
				string s = names[i];

				if (string.IsNullOrEmpty(s))
					continue;

				for (int j = 0; j < PartLoader.LoadedPartsList.Count; j++)
				{
					AvailablePart p = PartLoader.LoadedPartsList[j];

					if (p == null)
						continue;

					if (!ResearchAndDevelopment.PartModelPurchased(p))
						continue;

					if (p.partPrefab == null)
						continue;

					if (!p.partPrefab.Modules.Contains(s))
						continue;

					l.Add(p.title);
				}
			}

			return l;
		}

		internal void paramRewards(ContractParameter cP)
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractReward, (float)cP.FundsCompletion, cP.ScienceCompletion, cP.ReputationCompletion);

			fundsRewString = "";
			if (cP.FundsCompletion != 0)
				fundsRewString = "+ " + cP.FundsCompletion.ToString("N0");
			float fundsRewStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if (fundsRewStrat != 0f)
			{
				fundsRewString = string.Format("+ {0:N0} ({1:N0})", cP.FundsCompletion + fundsRewStrat, fundsRewStrat);
			}

			repRewString = "";
			if (cP.ReputationCompletion != 0)
				repRewString = "+ " + cP.ReputationCompletion.ToString("N0");
			float repRewStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if (repRewStrat != 0f)
			{
				repRewString = string.Format("+ {0:N0} ({1:N0})", cP.ReputationCompletion + repRewStrat, repRewStrat);
			}

			sciRewString = "";
			if (cP.ScienceCompletion != 0)
				sciRewString = "+ " + cP.ScienceCompletion.ToString("N0");
			float sciRewStrat = currencyQuery.GetEffectDelta(Currency.Science);
			if (sciRewStrat != 0f)
			{
				sciRewString = string.Format("+ {0:N0} ({1:N0})", cP.ScienceCompletion + sciRewStrat, sciRewStrat);
			}
		}

		internal void paramPenalties(ContractParameter cP)
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractPenalty, (float)cP.FundsFailure, 0f, cP.ReputationFailure);

			fundsPenString = "";
			if (cP.FundsFailure != 0)
				fundsPenString = "- " + cP.FundsFailure.ToString("N0");
			float fundsPenStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if (fundsPenStrat != 0f)
			{
				fundsPenString = string.Format("- {0:N0} ({1:N0})", cP.FundsFailure + fundsPenStrat, fundsPenStrat);
			}

			repPenString = "";
			if (cP.ReputationFailure != 0)
				repPenString = "- " + cP.ReputationFailure.ToString("N0");
			float repPenStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if (repPenStrat != 0f)
			{
				repPenString = string.Format("- {0:N0} ({1:N0})", cP.ReputationFailure + repPenStrat, repPenStrat);
			}
		}

		private void addSubParam(ContractParameter param, int Level)
		{
			//string partTest = contractScenario.paramTypeCheck(param);
			paramList.Add(new parameterContainer(root, param, Level, ""));
			root.addToParamList(paramList.Last());
		}

		public ContractParameter CParam
		{
			get { return cParam; }
		}

		public bool ShowNote
		{
			get { return showNote; }
			internal set { showNote = value; }
		}

		public string Title
		{
			get { return title; }
			internal set { title = value; }
		}

		public string Notes
		{
			get { return notes + customNotes; }
			internal set { notes = value; }
		}

		public int Level
		{
			get { return level; }
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

		public AvailablePart Part
		{
			get { return part; }
		}

		public List<parameterContainer> ParamList
		{
			get { return paramList; }
		}


	}
}
