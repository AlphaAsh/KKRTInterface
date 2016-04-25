using System;
using System.Collections.Generic;
using LibNoise.Unity.Operator;
using UnityEngine;
using System.Linq;
using System.IO;
using KerbalKonstructs;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.API;
using RemoteTech;
using RemoteTech.API;

namespace KerKonRTInterface.Utilities
{
	class PersistenceUtils
	{
		public static void UpdateRTGroundStations()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;
			
			string sGuID;
			string sTSName;
			string sDLat;
			string sDLon;
			string sHeight;

			foreach (StaticObject obj in KerbalKonstructs.KerbalKonstructs.instance.getStaticDB().getAllStatics())
			{
				if ((string)obj.getSetting("FacilityType") == "TrackingStation")
				{
					if ((string)obj.getSetting("OpenCloseState") == "Open")
					{
						sGuID = obj.getSetting("RadialPosition").ToString();
						sGuID = sGuID.Trim(new Char[] { ' ', ',', '.', '(', ')' });
						sGuID = sGuID.Replace(" ", "").Replace(",", "").Replace(".", "").Replace(")", "").Replace("(", "").Replace("-", "1");

						CelestialBody CelBody = (CelestialBody)obj.getSetting("CelestialBody");
						var objectpos = CelBody.transform.InverseTransformPoint(obj.gameObject.transform.position);
						var dObjectLat = NavUtils.GetLatitude(objectpos);
						var dObjectLon = NavUtils.GetLongitude(objectpos);
						var disObjectLat = dObjectLat * 180 / Math.PI;
						var disObjectLon = dObjectLon * 180 / Math.PI;

						sDLat = disObjectLat.ToString();
						sDLon = disObjectLon.ToString();
						sTSName = "Stn at Lt " + sDLat + " Ln " + sDLon;
						sHeight = obj.getSetting("RadiusOffset").ToString();

						Guid NewGuid = RemoteTech.API.API.AddGroundStation(sTSName, disObjectLat, disObjectLon, (double)obj.getSetting("RadiusOffset"), 1);
						obj.setSetting("RTGuid", NewGuid);
					}
					else
					{
						var gGuid = (Guid)obj.getSetting("RTGuid");
						RemoteTech.API.API.RemoveGroundStation(gGuid);
					}
				}
			}

			saveRTCareerBackup();
		}

		public static void saveRTCareerBackup()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			string rtPath = string.Format("{0}GameData/RemoteTech/", KSPUtil.ApplicationRootPath);
			string rtConfigPath = string.Format("{0}GameData/RemoteTech/RemoteTech_Settings.cfg", KSPUtil.ApplicationRootPath);
			if (!Directory.Exists(rtPath)) return;
			if (!File.Exists(rtConfigPath)) return;

			// Back it up
			string backupConfigPath = string.Format("{0}saves/{1}/KKRTCareerBack.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);
			File.Copy(rtConfigPath, backupConfigPath, true);
		}

		public static void loadRTCareerBackup()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			string rtPath = string.Format("{0}GameData/RemoteTech/", KSPUtil.ApplicationRootPath);
			string rtConfigPath = string.Format("{0}GameData/RemoteTech/RemoteTech_Settings.cfg", KSPUtil.ApplicationRootPath);
			if (!Directory.Exists(rtPath)) return;
			if (!File.Exists(rtConfigPath)) return;

			// Write the backup over the current
			string backupConfigPath = string.Format("{0}saves/{1}/KKRTCareerBack.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);
			if (!File.Exists(backupConfigPath)) return;
			File.Copy(backupConfigPath, rtConfigPath, true);

			UpdateRTGroundStations();
		}

		public static void backupRemoteTechConfig()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			string saveConfigPath = string.Format("{0}GameData/RemoteTech/RemoteTech_Settings.cfg", KSPUtil.ApplicationRootPath);
			string backupConfigPath = KerKonRTInterface.installDir + "/KKRTBack.cfg";

			if (File.Exists(saveConfigPath))
				File.Copy(saveConfigPath, backupConfigPath, true);
		}

		public static void restoreRemoteTechConfig()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			string saveConfigPath = KerKonRTInterface.installDir + "/KKRTBack.cfg";
			string restoreConfigPath = string.Format("{0}GameData/RemoteTech/RemoteTech_Settings.cfg", KSPUtil.ApplicationRootPath);

			if (File.Exists(saveConfigPath) && File.Exists(restoreConfigPath))
				File.Copy(saveConfigPath, restoreConfigPath, true);
		}
	}
}