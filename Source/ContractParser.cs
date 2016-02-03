using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Contracts;
using FinePrint.Contracts;

namespace ContractParser
{
    public static class contractParser
    {
		private static Dictionary<Guid, contractContainer> activeContracts = new Dictionary<Guid, contractContainer>();
		private static Dictionary<Guid, contractContainer> offeredContracts = new Dictionary<Guid, contractContainer>();
		private static Dictionary<Guid, contractContainer> completedContracts = new Dictionary<Guid, contractContainer>();
		private static Dictionary<Guid, contractContainer> failedContracts = new Dictionary<Guid, contractContainer>();

		public static contractContainer getActiveContract(Guid id, bool warn = false)
		{
			if (activeContracts.ContainsKey(id))
				return activeContracts[id];
			else if (warn)
				Debug.Log(string.Format("No Active Contract Of ID: [{0}] Found", id));

			return null;
		}

		public static bool addActiveContract(contractContainer c, bool warn = false)
		{
			if (!activeContracts.ContainsKey(c.ID))
			{
				activeContracts.Add(c.ID, c);
				return true;
			}
			else if (warn)
				Debug.Log(string.Format("Active Contract List Already Has Contract [{0} ; ID: {1}]", c.Title, c.ID));

			return false;
		}

		public static bool removeActiveContract(contractContainer c, bool warn = false)
		{
			if (activeContracts.ContainsKey(c.ID))
			{
				activeContracts.Remove(c.ID);
				return true;
			}
			else if (warn)
				Debug.Log(string.Format("Contract Not Found In Active Contract List [{0} ; ID: {1}]", c.Title, c.ID));

			return false;
		}

		public static contractContainer getOfferedContract(Guid id, bool warn = false)
		{
			if (offeredContracts.ContainsKey(id))
				return offeredContracts[id];
			else if (warn)
				Debug.Log(string.Format("No Offered Contract Of ID: [{0}] Found", id));

			return null;
		}

		public static bool addOfferedContract(contractContainer c, bool warn = false)
		{
			if (!offeredContracts.ContainsKey(c.ID))
			{
				offeredContracts.Add(c.ID, c);
				return true;
			}
			else if (warn)
				Debug.Log(string.Format("Offered Contract List Already Has Contract [{0} ; ID: {1}]", c.Title, c.ID));

			return false;
		}

		public static bool removeOfferedContract(contractContainer c, bool warn = false)
		{
			if (offeredContracts.ContainsKey(c.ID))
			{
				offeredContracts.Remove(c.ID);
				return true;
			}
			else if (warn)
				Debug.Log(string.Format("Contract Not Found In Offered Contract List [{0} ; ID: {1}]", c.Title, c.ID));

			return false;
		}

		public static contractContainer getCompletedContract(Guid id, bool warn = false)
		{
			if (completedContracts.ContainsKey(id))
				return completedContracts[id];
			else if (warn)
				Debug.Log(string.Format("No Completed Contract Of ID: [{0}] Found", id));

			return null;
		}

		public static bool addCompletedContract(contractContainer c, bool warn = false)
		{
			if (!completedContracts.ContainsKey(c.ID))
			{
				completedContracts.Add(c.ID, c);
				return true;
			}
			else if (warn)
				Debug.Log(string.Format("Completed Contract List Already Has Contract [{0} ; ID: {1}]", c.Title, c.ID));

			return false;
		}

		public static bool removeCompletedContract(contractContainer c, bool warn = false)
		{
			if (completedContracts.ContainsKey(c.ID))
			{
				completedContracts.Remove(c.ID);
				return true;
			}
			else if (warn)
				Debug.Log(string.Format("Contract Not Found In Completed Contract List [{0} ; ID: {1}]", c.Title, c.ID));

			return false;
		}

		public static contractContainer getFailedContract(Guid id, bool warn = false)
		{
			if (failedContracts.ContainsKey(id))
				return failedContracts[id];
			else if (warn)
				Debug.Log(string.Format("No Failed Contract Of ID: [{0}] Found", id));

			return null;
		}

		public static bool addFailedContract(contractContainer c, bool warn = false)
		{
			if (!failedContracts.ContainsKey(c.ID))
			{
				failedContracts.Add(c.ID, c);
				return true;
			}
			else if (warn)
				Debug.Log(string.Format("Failed Contract List Already Has Contract [{0} ; ID: {1}]", c.Title, c.ID));

			return false;
		}

		public static bool removeFailedContract(contractContainer c, bool warn = false)
		{
			if (failedContracts.ContainsKey(c.ID))
			{
				failedContracts.Remove(c.ID);
				return true;
			}
			else if (warn)
				Debug.Log(string.Format("Contract Not Found In Failed Contract List [{0} ; ID: {1}]", c.Title, c.ID));

			return false;
		}

		public static List<contractContainer> getActiveContracts
		{
			get { return activeContracts.Values.ToList(); }
		}

		public static List<contractContainer> getOfferedContracts
		{
			get { return offeredContracts.Values.ToList(); }
		}

		public static List<contractContainer> getCompletedContracts
		{
			get { return completedContracts.Values.ToList(); }
		}

		public static List<contractContainer> getFailedContracts
		{
			get { return failedContracts.Values.ToList(); }
		}
    }
}
