using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using KerKonRTInterface.Utilities;
using System.Reflection;
using KSP.UI.Screens;
using KerbalKonstructs;
using KerbalKonstructs.StaticObjects;
using RemoteTech;
using RemoteTech.API;

namespace KerKonRTInterface
{
	[KSPAddonFixed(KSPAddon.Startup.MainMenu, true, typeof(KerKonRTInterface))]
	public class KerKonRTInterface : MonoBehaviour
	{
		// Hello
		public static KerKonRTInterface instance;		
		public static string installDir = AssemblyLoader.loadedAssemblies.GetPathByType(typeof(KerKonRTInterface));

		public StaticDatabase staticDB = new StaticDatabase();

		void Awake()
		{
			instance = this;

			#region Game Event Hooks
			GameEvents.onGameStateSave.Add(SaveState);
			GameEvents.onGameStateLoad.Add(LoadState);
			#endregion
			
			DontDestroyOnLoad(this);

			// Subscribe to KK events
			KerbalKonstructs.UI.SharedInterfaces.evFacilityOpened += PersistenceUtils.UpdateRTGroundStations;
			KerbalKonstructs.UI.SharedInterfaces.evFacilityClosed += PersistenceUtils.UpdateRTGroundStations;
		}

		#region Game Events

		public void LoadState(ConfigNode configNode)
		{
			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{}
			else
			{
				PersistenceUtils.loadRTCareerBackup();
			}
		}

		public void SaveState(ConfigNode configNode)
		{
			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{}
			else
			{
				PersistenceUtils.saveRTCareerBackup();
			}
		}

		#endregion
	}
}
