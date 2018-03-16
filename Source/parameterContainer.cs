#region license
/*The MIT License (MIT)
Parameter Container - An object for storing information about contract parameters

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
using System.Reflection;
using Contracts;
using FinePrint;
using FinePrint.Contracts;
using FinePrint.Contracts.Parameters;
using UnityEngine;
using KSP.Localization;
using SentinelMission;

namespace ContractParser
{
	public class parameterContainer
	{
		private contractContainer root;
		private ContractParameter cParam;
		private Type cParamType;
		private bool showNote;
		private string title;
		private string notes;
		private string customNotes;
		private int level;
		private float fundsRew, fundsPen, repRew, repPen, sciRew;
		private float fundsRewStrat, fundsPenStrat, repRewStrat, repPenStrat, sciRewStrat;
		private string fundsRewString, fundsPenString, repRewString, repPenString, sciRewString;
		private Waypoint waypoint;
		private List<parameterContainer> paramList = new List<parameterContainer>();

		private StringBuilder customNoteString;

		public parameterContainer(contractContainer Root, ContractParameter cP, int Level)
		{
			root = Root;
			cParam = cP;
			cParamType = cParam.GetType();

			try
			{
				title = cParam.Title;
			}
			catch (Exception e)
			{
				Debug.LogError("[Contract Parser] Contract Parameter Title not set, using type name...\n" + e);
				title = cParam.GetType().Name;
			}

			try
			{
				notes = cParam.Notes;
			}
			catch (Exception e)
			{
				Debug.LogError("[Contract Parser] Contract Parameter Notes not set, blank notes used...\n" + e);
				notes = "";
			}

			level = Level;	
			paramRewards();
			paramPenalties();

			waypoint = checkForWaypoint();

			customNotes = setCustomNotes();

			if (level < 4)
			{
				for (int i = 0; i < cParam.ParameterCount; i++)
				{
					ContractParameter param = cParam.GetParameter(i);
					addSubParam(param, level + 1);
				}
			}
		}

		private string setCustomNotes()
		{
			customNoteString = StringBuilderCache.Acquire();

			if (cParamType == typeof(PartRequestParameter))
			{
				List<string> l = new List<string>();

				var partFields = cParamType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				if (partFields == null)
					return "";

				try
				{
					l = (List<string>)partFields[4].GetValue((PartRequestParameter)cParam);
				}
				catch (Exception e)
				{
					Debug.LogError("[Contract Parser] Custom Notes: Error Detecting Acceptable Parts Name...\n" + e);
					return "";
				}

				if (l != null)
				{
					if (l.Count > 0)
					{
						List<string> titles = getPartTitles(l);

						if (titles.Count > 0)
						{
							if (!string.IsNullOrEmpty(notes))
							{
								customNoteString.AppendLine();
								customNoteString.AppendLine();
							}

							customNoteString.Append(Localizer.Format("#autoLOC_ContractParser_PartRequest"));

							for (int i = titles.Count - 1; i >= 0; i--)
							{
								string t = titles[i];

								customNoteString.AppendLine();
								customNoteString.Append(t);
							}
						}
					}
				}

				try
				{
					l = (List<string>)partFields[5].GetValue((PartRequestParameter)cParam);
				}
				catch (Exception e)
				{
					Debug.LogError("[Contract Parser] Custom Notes: Error Detecting Acceptable Part Modules Name...\n" + e);
					return "";
				}

				if (l != null)
				{
					if (l.Count > 0)
					{
						List<string> titles = getPartTitlesFromModules(l);

						if (titles.Count > 0)
						{
							if (customNoteString.Length == 0)
							{
								if (!string.IsNullOrEmpty(notes))
								{
									customNoteString.AppendLine();
									customNoteString.AppendLine();
								}

								customNoteString.Append(Localizer.Format("#autoLOC_ContractParser_PartRequest"));
							}

							for (int i = titles.Count - 1; i >= 0; i--)
							{
								string t = titles[i];

								customNoteString.AppendLine();
								customNoteString.Append(t);
							}
						}
					}
				}
			}
			else if (cParamType == typeof(VesselSystemsParameter))
			{
				if (!((VesselSystemsParameter)cParam).requireNew)
					return "";

				List<string> l = new List<string>();

				var modFields = cParamType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				if (modFields == null)
					return "";

				try
				{
					l = (List<string>)modFields[0].GetValue((VesselSystemsParameter)cParam);
				}
				catch (Exception e)
				{
					Debug.LogError("[Contract Parser] Custom Notes: Error Detecting Acceptable Module Type Name...\n" + e.ToString());
					return "";
				}

				if (l != null)
				{
					if (l.Count > 0)
					{
						for (int j = l.Count - 1; j >= 0; j--)
						{
							string m = l[j];

							List<string> titles = getPartTitlesFromModuleType(m);

							if (titles.Count > 0)
							{
								customNoteString.AppendLine();
								customNoteString.AppendLine();

								customNoteString.Append(Localizer.Format("#autoLOC_ContractParser_ModuleType", l[j]));

								for (int i = 0; i < titles.Count; i++)
								{
									string t = titles[i];

									customNoteString.AppendLine();
									customNoteString.Append(t);
								}
							}
						}
					}
				}
			}
			else if (cParamType == typeof(SentinelParameter))
			{
				int r = ((SentinelParameter)cParam).RemainingDiscoveries;
				int t = ((SentinelParameter)cParam).TotalDiscoveries;

				customNoteString.Append(Localizer.Format("#autoLOC_ContractParser_SentinalNote", t - r, t));
			}

			return customNoteString.ToStringAndRelease();
		}

		private List<string> getPartTitles(List<string> names)
		{
			List<string> l = new List<string>();

			for (int i = names.Count - 1; i >= 0; i--)
			{
				string s = names[i];

				if (string.IsNullOrEmpty(s))
					continue;

				AvailablePart p = PartLoader.getPartInfoByName(s.Replace('_', '.'));

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

				for (int j = PartLoader.LoadedPartsList.Count - 1; j >= 0; j--)
				{

					AvailablePart p = PartLoader.LoadedPartsList[j];

					if (p == null)
						continue;

					if (!ResearchAndDevelopment.PartModelPurchased(p))
						continue;

					if (p.partPrefab == null)
						continue;

					try
					{
						int hash = s.GetHashCode();

						for (int k = p.partPrefab.Modules.Count - 1; k >= 0; k--)
						{
							PartModule pm = p.partPrefab.Modules[k];

							if (pm == null)
								continue;

							if (pm.ModuleAttributes == null)
								continue;
							
							if (pm.ClassID != hash)
								continue;

							l.Add(p.title);
							break;
						}
					}
					catch (Exception e)
					{
						Debug.LogError("[Contract Parser] Custom Notes: Error Parsing Part: [" + p.name + "] For Module: [" + s + "]\n" + e.ToString());
					}
				}
			}

			return l;
		}

		private List<string> getPartTitlesFromModuleType(string type)
		{
			List<string> l = new List<string>();

			for (int i = PartLoader.LoadedPartsList.Count - 1; i >= 0; i--)
			{
				AvailablePart p = PartLoader.LoadedPartsList[i];

				if (p == null)
					continue;

				if (p.partPrefab == null)
					continue;

				try
				{
					if (!p.partPrefab.HasValidContractObjective(type))
						continue;

					if (!ResearchAndDevelopment.PartModelPurchased(p))
						continue;

					l.Add(p.title);
				}
				catch (Exception e)
				{
					Debug.LogError("[Contract Parser] Custom Notes: Error Parsing Part: [" + p.name + "] For Module Type: ["+ type + "]\n" + e.ToString());
				}
			}

			return l;
		}

		private Waypoint checkForWaypoint()
		{
			Waypoint p = null;

			if (cParam.GetType() == typeof(SurveyWaypointParameter))
			{
				SurveyWaypointParameter s = (SurveyWaypointParameter)cParam;

				if (s.State != ParameterState.Incomplete)
					return p;

				return s.wp;
			}

			else if (cParam.GetType() == typeof(StationaryPointParameter))
			{
				StationaryPointParameter s = (StationaryPointParameter)cParam;
				if (s.State != ParameterState.Incomplete)
					return p;

				try
				{
					var field = (typeof(StationaryPointParameter)).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)[0];
					p = (Waypoint)field.GetValue(s);
				}
				catch (Exception e)
				{
					Debug.Log(string.Format("[Contract Parser] Error While Assigning FinePrint Stationary Waypoint Object\n{0}", e));
				}
			}

			return p;
		}

		internal void paramRewards()
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractReward, (float)cParam.FundsCompletion, cParam.ScienceCompletion, cParam.ReputationCompletion);

			fundsRew = (float)cParam.FundsCompletion;
			fundsRewStrat = currencyQuery.GetEffectDelta(Currency.Funds);

			if (fundsRewStrat != 0)
				fundsRewString = string.Format("+ {0:N0} ({1:N0})", fundsRew + fundsRewStrat, fundsRewStrat);
			else if (fundsRew != 0)
				fundsRewString = "+ " + fundsRew.ToString("N0");
			else
				fundsRewString = "";

			repRew = cParam.ReputationCompletion;
			repRewStrat = currencyQuery.GetEffectDelta(Currency.Reputation);

			if (repRewStrat != 0)
				repRewString = string.Format("+ {0:N0} ({1:N0})", repRew + repRewStrat, repRewStrat);
			else if (repRew != 0)
				repRewString = "+ " + repRew.ToString("N0");
			else
				repRewString = "";

			sciRew = cParam.ScienceCompletion;
			sciRewStrat = currencyQuery.GetEffectDelta(Currency.Science);

			if (sciRewStrat != 0)
				sciRewString = string.Format("+ {0:N0} ({1:N0})", sciRew + sciRewStrat, sciRewStrat);
			else if (sciRew != 0)
				sciRewString = "+ " + sciRew.ToString("N0");
			else
				sciRewString = "";
		}

		internal void paramPenalties()
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractPenalty, (float)cParam.FundsFailure, 0f, cParam.ReputationFailure);

			fundsPen = (float)cParam.FundsFailure;
			fundsPenStrat = currencyQuery.GetEffectDelta(Currency.Funds);

			if (fundsPenStrat != 0)
				fundsPenString = string.Format("- {0:N0} ({1:N0})", fundsPen + fundsPenStrat, fundsPenStrat);
			else if (fundsPen != 0)
				fundsPenString = "- " + fundsPen.ToString("N0");
			else
				fundsPenString = "";

			repPen = cParam.ReputationFailure;
			repPenStrat = currencyQuery.GetEffectDelta(Currency.Reputation);

			if (repPenStrat != 0)
				repPenString = string.Format("- {0:N0} ({1:N0})", repPen + repPenStrat, repPenStrat);
			else if (repPen != 0)
				repPenString = "- " + repPen.ToString("N0");
			else
				repPenString = "";
		}

		private void addSubParam(ContractParameter param, int Level)
		{
			paramList.Add(new parameterContainer(root, param, Level));
			root.addToParamList(paramList.Last());
		}

		public int ParameterCount
		{
			get { return paramList.Count; }
		}

		public parameterContainer getParameter(int index)
		{
			if (paramList.Count > index)
				return paramList[index];

			return null;
		}

		public ContractParameter CParam
		{
			get { return cParam; }
		}

		public Type CParamType
		{
			get { return cParamType; }
		}

		public Waypoint Way
		{
			get { return waypoint; }
		}

		public bool ShowNote
		{
			get { return showNote; }
			set { showNote = value; }
		}

		public string Title
		{
			get { return title; }
			set { title = value; }
		}

		public string Notes(bool custom = false)
		{
			if (custom)
				return notes + customNotes;

			return notes;
		}

		public void setNotes(string s)
		{
			notes = s;
		}

		public int Level
		{
			get { return level; }
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

		public List<parameterContainer> ParamList
		{
			get { return paramList; }
		}

	}
}
