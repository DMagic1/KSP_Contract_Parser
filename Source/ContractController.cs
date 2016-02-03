using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Contracts;


namespace ContractParser
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class contractController : MonoBehaviour
	{
		private static bool initialized;

		public static contractController instance;

		private void Start()
		{
			if (initialized)
				Destroy(gameObject);

			instance = this;

			DontDestroyOnLoad(gameObject);

			initialized = true;

			GameEvents.Contract.onParameterChange.Add(onParamChange);
			GameEvents.Contract.onAccepted.Add(onAccepted);
			GameEvents.Contract.onDeclined.Add(onDeclined);
			GameEvents.Contract.onFinished.Add(onFinished);
			GameEvents.Contract.onOffered.Add(onOffered);
			GameEvents.Contract.onContractsLoaded.Add(onContractsLoaded);
			GameEvents.Contract.onContractsListChanged.Add(onListChanged);
		}

		private void onParamChange(Contract c, ContractParameter p)
		{
			contractContainer cc = contractParser.getActiveContract(c.ContractGuid);

			if (cc == null)
				return;

			if (c.AllParameters.Count() > cc.ParameterCount)
				cc.updateFullParamInfo();
		}

		private void onAccepted(Contract c)
		{
			if (c == null)
			{
				Debug.Log("Error in loading null accepted contract");
				return;
			}

			contractContainer cc = contractParser.getOfferedContract(c.ContractGuid, true);

			if (cc == null)
				return;

			cc.updateTimeValues();

			contractParser.removeOfferedContract(cc, true);

			contractParser.addActiveContract(cc, true);
			refreshList();
		}

		private void onDeclined(Contract c)
		{
			if (c == null)
			{
				Debug.Log("Error in loading null declined contract");
				return;
			}

			contractContainer cc = contractParser.getOfferedContract(c.ContractGuid, true);

			if (cc == null)
				return;

			contractParser.removeOfferedContract(cc, true);
			refreshList();
		}

		private void onFinished(Contract c)
		{
			if (c == null)
			{
				Debug.Log("Error in loading null finished contract");
				return;
			}

			contractContainer cc = contractParser.getActiveContract(c.ContractGuid);

			if (cc == null)
				cc = contractParser.getOfferedContract(c.ContractGuid, true);

			if (cc == null)
				return;

			cc.updateTimeValues();

			contractParser.removeOfferedContract(cc);
			contractParser.removeActiveContract(cc);
			if (c.ContractState == Contract.State.Completed)
				contractParser.addCompletedContract(cc, true);
			refreshList();
		}

		private void onOffered(Contract c)
		{
			if (c == null)
			{
				Debug.Log("Error in loading null offered contract");
				return;
			}

			contractContainer cc = new contractContainer(c);

			if (cc == null)
				return;

			contractParser.addOfferedContract(cc, true);
			refreshList();
		}

		private void onContractsLoaded()
		{
			StartCoroutine(loadContracts());
		}

		private void onListChanged()
		{
			refreshList();
		}

		private IEnumerator loadContracts()
		{
			yield break;
		}

		private void refreshList()
		{

		}

	}
}
