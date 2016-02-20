#region license
/*The MIT License (MIT)
Contract Controller - A Monobehaviour for monitoring contract activity and loading the parser

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

			Debug.Log("[Contract Parser] Starting Contract Parsing Controller...");
			initialized = true;

			GameEvents.Contract.onParameterChange.Add(onParamChange);
			GameEvents.Contract.onAccepted.Add(onAccepted);
			GameEvents.Contract.onDeclined.Add(onDeclined);
			GameEvents.Contract.onFinished.Add(onFinished);
			GameEvents.Contract.onOffered.Add(onOffered);
			GameEvents.Contract.onContractsLoaded.Add(onContractsLoaded);
			GameEvents.onGameSceneSwitchRequested.Add(onSceneChange);
		}

		private void onSceneChange(GameEvents.FromToAction<GameScenes, GameScenes> g)
		{
			contractParser.Loaded = false;
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
				Debug.Log("[Contract Parser] Error in loading null accepted contract");
				return;
			}

			contractContainer cc = contractParser.getOfferedContract(c.ContractGuid, true);

			if (cc == null)
				return;

			cc.updateTimeValues();

			contractParser.removeOfferedContract(cc, true);

			contractParser.addActiveContract(cc, true);

			contractParser.onContractStateChange.Fire(c);
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

			contractParser.onContractStateChange.Fire(c);
		}

		private void onFinished(Contract c)
		{
			if (c == null)
			{
				Debug.Log("[Contract Parser] Error in loading null finished contract");
				return;
			}

			contractContainer cc = contractParser.getActiveContract(c.ContractGuid);

			if (cc == null)
				cc = contractParser.getOfferedContract(c.ContractGuid);

			if (cc == null)
				return;

			cc.updateTimeValues();

			contractParser.removeOfferedContract(cc);
			contractParser.removeActiveContract(cc);
			if (c.ContractState == Contract.State.Completed)
				contractParser.addCompletedContract(cc, true);

			contractParser.onContractStateChange.Fire(c);
		}

		private void onOffered(Contract c)
		{
			if (c == null)
			{
				Debug.Log("[Contract Parser] Error in loading null offered contract");
				return;
			}

			contractContainer cc = new contractContainer(c);

			if (cc == null)
				return;

			contractParser.addOfferedContract(cc, true);

			contractParser.onContractStateChange.Fire(c);
		}

		private void onContractsLoaded()
		{
			StartCoroutine(contractParser.loadContracts());
		}

	}
}
