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
	class KKRTPersistenceUtils
	{
		public static void InitialiseRTGroundStations()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			foreach (StaticObject obj in KerbalKonstructs.KerbalKonstructs.instance.getStaticDB().getAllStatics())
			{
				if ((string)obj.getSetting("FacilityType") == "TrackingStation")
				{
					if ((string)obj.getSetting("OpenCloseState") == "Open")
						OpenRTGroundStation(obj);
				}
			}
		}

		public static void OpenRTGroundStation()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			StaticObject obj = KerbalKonstructs.UI.SharedInterfaces.getStoredEventObject();
			if (obj == null)
			{
				Debug.Log("KK: Don't know what static object just got opened.");
				return;
			}

			KerbalKonstructs.Utilities.PersistenceUtils.loadStaticPersistence(obj);

			string sTSName;
			string sDLat;
			string sDLon;
			string sHeight;

			if ((string)obj.getSetting("FacilityType") == "TrackingStation")
			{
				CelestialBody CelBody = (CelestialBody)obj.getSetting("CelestialBody");
				var objectpos = CelBody.transform.InverseTransformPoint(obj.gameObject.transform.position);
				var dObjectLat = NavUtils.GetLatitude(objectpos);
				var dObjectLon = NavUtils.GetLongitude(objectpos);
				double disObjectLat = dObjectLat * 180 / Math.PI;
				double disObjectLon = dObjectLon * 180 / Math.PI;

				sDLat = disObjectLat.ToString("#0.00");
				sDLon = disObjectLon.ToString("#0.00");
				sTSName = "Stn Lt " + sDLat + " Ln " + sDLon;
				sHeight = obj.getSetting("RadiusOffset").ToString();

				float fHeight = (float)obj.getSetting("RadiusOffset");
				double dHeight = (double)fHeight;
				int iBody = 1;
				Guid NewGuid;
				Guid gGuid = (Guid)obj.getSetting("RTGuid");

				if (gGuid == Guid.Empty)
				{
					NewGuid = RemoteTech.API.API.AddGroundStation(sTSName, disObjectLat, disObjectLon, dHeight, iBody);

					if (NewGuid == Guid.Empty)
					{
						Debug.Log("KK: RT handed back an empty guid!");
					}

					obj.setSetting("RTGuid", NewGuid);
					Debug.Log("KK: Added RT GroundStation " + NewGuid.ToString());
					KerbalKonstructs.Utilities.PersistenceUtils.saveStaticPersistence(obj);
					RemoteTech.API.API.DirectConfigureStationAntenna(NewGuid, (float)obj.getSetting("TrackingShort"), 0, 1);
					Debug.Log("KK: Saved station object");
				}
				else
				{
					Debug.Log("KK: The station just opened already had a guid???");
				}
			}
		}

		public static string ConstructGuidString(StaticObject obj)
		{
			string sGuid = "";

			Vector3 vPos = (Vector3)obj.getSetting("RadialPosition");
			string sPos = vPos.ToString();

			string sHeight = obj.getSetting("RadiusOffset").ToString();

			sGuid = sHeight.Replace(".", "") + sPos.Replace("@", "").Replace(",", "").Replace(".", "").Replace(";", "").Replace("'", "").Replace(" ", "");
			return sGuid;
		}

		public static void OpenRTGroundStation(StaticObject obj)
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			string sTSName;
			string sDLat;
			string sDLon;
			string sHeight;

			if ((string)obj.getSetting("FacilityType") == "TrackingStation")
			{
				Vector3 vPos = (Vector3)obj.getSetting("RadialPosition");
				string sPos = vPos.ToString();
				CelestialBody CelBody = (CelestialBody)obj.getSetting("CelestialBody");
				var objectpos = CelBody.transform.InverseTransformPoint(obj.gameObject.transform.position);
				var dObjectLat = NavUtils.GetLatitude(objectpos);
				var dObjectLon = NavUtils.GetLongitude(objectpos);
				double disObjectLat = dObjectLat * 180 / Math.PI;
				double disObjectLon = dObjectLon * 180 / Math.PI;

				sDLat = disObjectLat.ToString("#0.00");
				sDLon = disObjectLon.ToString("#0.00");
				sTSName = "Stn Lt " + sDLat + " Ln " + sDLon;
				sHeight = obj.getSetting("RadiusOffset").ToString();

				float fHeight = (float)obj.getSetting("RadiusOffset");
				double dHeight = (double)fHeight;
				int iBody = 1;

				string sGuid = ConstructGuidString(obj);

				RemoteTech.API.API.AddGroundStation(sGuid, sTSName, disObjectLat, disObjectLon, dHeight, iBody);

				Debug.Log("KK: Added RT GroundStation " + sGuid);
				KerbalKonstructs.Utilities.PersistenceUtils.saveStaticPersistence(obj);
				Debug.Log("KK: Saved station object");
			}
		}

		public static void CloseRTGroundStation(StaticObject obj)
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;
			if (obj == null) return;

			if ((string)obj.getSetting("FacilityType") == "TrackingStation")
			{
				string sGuid = ConstructGuidString(obj);
				
				if (sGuid != "")
				{
					Debug.Log("KK: Attempting to remove RT GroundStation");
					Guid nGuid = new Guid(sGuid);
					bool bRemove = RemoteTech.API.API.RemoveGroundStation(nGuid);
					if (bRemove)
						Debug.Log("KK: Removed RT GroundStation");
					else
						Debug.Log("KK: Failed to remove RT GroundStation " + sGuid);

					KerbalKonstructs.Utilities.PersistenceUtils.saveStaticPersistence(obj);
					Debug.Log("KK: Saved station object");
				}
				else
				{
					Debug.Log("KK: Can't remove RT GroundStation because it has no Guid");
				}
			}
		}

		/* public static void CloseRTGroundStation(StaticObject obj, Guid gGuid)
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			if ((string)obj.getSetting("FacilityType") == "TrackingStation")
			{
				if (gGuid != Guid.Empty)
				{
					RemoteTech.API.API.RemoveGroundStation(gGuid);
				}
			}
		} */

		public static void UpdateRTGroundStations()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			foreach (StaticObject obj in KerbalKonstructs.KerbalKonstructs.instance.getStaticDB().getAllStatics())
			{
				if ((string)obj.getSetting("FacilityType") == "TrackingStation")
				{
					KerbalKonstructs.Utilities.PersistenceUtils.loadStaticPersistence(obj);
					
					if ((string)obj.getSetting("OpenCloseState") == "Open")
					{
						OpenRTGroundStation(obj);
					}
				}
			}

			foreach (StaticObject obj in KerbalKonstructs.KerbalKonstructs.instance.getStaticDB().getAllStatics())
			{
				if ((string)obj.getSetting("FacilityType") == "TrackingStation")
				{
					if ((string)obj.getSetting("OpenCloseState") == "Open")
					{
					}
					else
					{
						KerbalKonstructs.Utilities.PersistenceUtils.loadStaticPersistence(obj);
						CloseRTGroundStation(obj);
					}
				}
			}
		}

		/* public static void saveRTCareerBackup()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			string rtPath = string.Format("{0}GameData/RemoteTech/", KSPUtil.ApplicationRootPath);
			string rtConfigPath = string.Format("{0}GameData/RemoteTech/Default_Settings.cfg", KSPUtil.ApplicationRootPath);
			if (!Directory.Exists(rtPath)) return;
			if (!File.Exists(rtConfigPath)) return;

			// Back it up
			string backupConfigPath = string.Format("{0}saves/{1}/KKRTCareerBack.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);
			File.Copy(rtConfigPath, backupConfigPath, true);
		} */

		/* public static void loadRTCareerBackup()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			string rtPath = string.Format("{0}GameData/RemoteTech/", KSPUtil.ApplicationRootPath);
			string rtConfigPath = string.Format("{0}GameData/RemoteTech/Default_Settings.cfg", KSPUtil.ApplicationRootPath);
			if (!Directory.Exists(rtPath)) return;
			if (!File.Exists(rtConfigPath)) return;

			// Write the backup over the current
			string backupConfigPath = string.Format("{0}saves/{1}/KKRTCareerBack.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);
			if (!File.Exists(backupConfigPath)) return;
			File.Copy(backupConfigPath, rtConfigPath, true);
		} */

		/* public static void backupRemoteTechConfig()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			string saveConfigPath = string.Format("{0}GameData/RemoteTech/Default_Settings.cfg", KSPUtil.ApplicationRootPath);
			string backupConfigPath = KerKonRTInterface.installDir + "/KKRTBack.cfg";

			if (File.Exists(saveConfigPath))
				File.Copy(saveConfigPath, backupConfigPath, true);
		}

		public static void restoreRemoteTechConfig()
		{
			bool bRTLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.name == "RemoteTech");
			if (!bRTLoaded) return;

			string saveConfigPath = KerKonRTInterface.installDir + "/KKRTBack.cfg";
			string restoreConfigPath = string.Format("{0}GameData/RemoteTech/Default_Settings.cfg", KSPUtil.ApplicationRootPath);

			if (File.Exists(saveConfigPath) && File.Exists(restoreConfigPath))
				File.Copy(saveConfigPath, restoreConfigPath, true);
		} */
	}
}