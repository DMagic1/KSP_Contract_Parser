using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Contracts;
using UnityEngine;

namespace ContractParser
{
	public static class contractReflection
	{
		internal static void loadMethods()
		{
			ccLoaded = loadCCPendingMethod();
		}

		private static bool ccLoaded;

		public static bool CCLoaded
		{
			get { return ccLoaded; }
		}

		private const string CCType = "ContractConfigurator.ConfiguredContract";
		private const string PendingName = "CurrentContracts";

		private delegate IEnumerable CCPendingContracts();

		private static CCPendingContracts _CCPendingContracts;

		internal static List<Contract> pendingContracts()
		{
			IEnumerable generic = _CCPendingContracts();
			List<Contract> pendingContractList = new List<Contract>();

			foreach (System.Object obj in generic)
			{
				if (obj == null)
					continue;
				Contract c = obj as Contract;

				pendingContractList.Add(c);
			}

			return pendingContractList;
		}

		private static bool loadCCPendingMethod()
		{
			try
			{
				Type CConfigType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes())
						.SingleOrDefault(t => t.FullName == CCType);

				if (CConfigType == null)
				{
					Debug.Log(string.Format("[Contract Parser] Contract Configurator Type [{0}] Not Found", CCType));
					return false;
				}

				PropertyInfo CCPending = CConfigType.GetProperty(PendingName);

				if (CCPending == null)
				{
					Debug.Log(string.Format("[Contract Parser] Contract Configurator Property [{0}] Not Loaded", PendingName));
					return false;
				}

				_CCPendingContracts = (CCPendingContracts)Delegate.CreateDelegate(typeof(CCPendingContracts), CCPending.GetGetMethod());

				Debug.Log("[Contract Parser] Contract Configurator Pending Contracts Method Assigned");

				return _CCPendingContracts != null;
			}
			catch (Exception e)
			{
				Debug.Log(string.Format("[Contract Parser] Error in loading Contract Configurator methods\n{0}", e));
				return false;
			}
		}
	}
}
