using ICities;
using UnityEngine;
using System;
using ColossalFramework;
using ColossalFramework.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//Author: Juuso "Zuppi" Hietala
//Version 1.3
namespace ChirpFilter
{
	
	public class ChirpFilterdesc : IUserMod 
	{
		
		public string Name
		{
			get { return "ChirpFilter"; }
		}
		
		public string Description 
		{
			get { return "Filters chirper to remove non-informative messages"; }
		}
	}
	
	public class ChirpFilterer : IChirperExtension
	{
		MessageManager messageManager;
		ChirpPanel cpanel;
		GameObject chirpContainer;
		private bool chiperChecked;
		private bool chirperDestroyed;
		private AudioClip soundStore;
		private List<FilterContainer> recentMessages;
		private FilterModule module;
		
		
		public void OnCreated(IChirper chirper)
		{
			chirperDestroyed = false;

			messageManager = GameObject.Find("MessageManager").GetComponent<MessageManager>();		
			GameObject filterModule = new GameObject("ChirperFilterModule");
			module = filterModule.AddComponent<ChirpFilter.FilterModule>();
			
			cpanel = GameObject.Find("ChirperPanel").GetComponent<ChirpPanel>();

			chirpContainer = cpanel.transform.FindChild("Chirps").FindChild("Clipper").FindChild("Container").gameObject;
			soundStore = cpanel.m_NotificationSound;
			chiperChecked = false;
			recentMessages = new List<FilterContainer>();
		}
		
		public void OnReleased()
		{

		}
		
		public void OnNewMessage(IChirperMessage msg)
		{
			if (!chiperChecked){
				cpanel.ClearMessages();
				chiperChecked = true;
				if (chirpContainer == null){
					chirperDestroyed = true;
				}
			}
			if (!chirperDestroyed){
				CitizenMessage cm = msg as CitizenMessage;
				if (cm != null){
					FilterContainer fc = new FilterContainer(cm, chirpContainer.transform.childCount);
					filter(fc);
					recentMessages.Add(fc);
				}


			}
		}
		
		public void OnMessagesUpdated()
		{
			
		}
		
		public void OnUpdate()
		{	
			if (!chirperDestroyed){
				if (recentMessages.Count != 0){
					if (recentMessages[0].filtered){
						cpanel.Collapse();
						cpanel.m_NotificationSound = soundStore;
						if (chirpContainer.transform.childCount > 0){
							for (int i=0;i<chirpContainer.transform.childCount;i++){
								if(chirpContainer.transform.GetChild(i).GetComponentInChildren<UILabel>().text.Equals(recentMessages[0].chirpmessagetext)){
									UITemplateManager.RemoveInstance ("ChirpTemplate", chirpContainer.transform.GetChild(i).GetComponent<UIPanel>());
									messageManager.DeleteMessage(recentMessages[0].cm);
									recentMessages.RemoveAt(0);
									break;
								}
							}
						}
						else{
							recentMessages.RemoveAt(0);
						}

					}
					else{
						if (chirpContainer.transform.childCount > 0){
							for (int i=0;i<chirpContainer.transform.childCount;i++){
								if(chirpContainer.transform.GetChild(i).GetComponentInChildren<UILabel>().text.Equals(recentMessages[0].chirpmessagetext)){
									chirpContainer.transform.GetChild(i).GetComponentInChildren<UILabel>().text = module.removeTwitterness(recentMessages[0].chirpmessagetext);
									break;
								}
							}
						}
						recentMessages.RemoveAt(0);
					}
				}
			}
		}


		private void filter(FilterContainer fc){
			if (module.filterContainer(fc)){
				cpanel.m_NotificationSound = null;
			}			
		}

	}

	public class FilterContainer{
		public bool filtered;
		public string chirpmessagetext;
		public int index;
		public CitizenMessage cm;
		
		public FilterContainer(CitizenMessage citizenmsg, int id){
			this.cm = citizenmsg;
			this.filtered = false;
			this.chirpmessagetext = cm.text;
			this.index = id;
			
		}
	}

	public class FilterModule : MonoBehaviour, IFormattable {
		
		public bool filterContainer(FilterContainer fc){
			if (checkCategory(fc.cm.m_messageID)){
				fc.filtered = true;
				return true;
			}
			else{
				return false;
			}
			
		}

		public string ToString(string s, IFormatProvider ifp){
			if (checkCategory(s)){
				return "true";
			}
			else{
				return "false";
			}
		}

		//Credit goes to https://github.com/mabako/reddit-for-city-skylines
		private bool checkCategory(String cat){
			switch(cat)
			{			
			case LocaleID.CHIRP_ABANDONED_BUILDINGS:
			case LocaleID.CHIRP_COMMERCIAL_DEMAND:
			case LocaleID.CHIRP_DEAD_PILING_UP:
			case LocaleID.CHIRP_FIRE_HAZARD:
			case LocaleID.CHIRP_HIGH_CRIME:
			case LocaleID.CHIRP_INDUSTRIAL_DEMAND:
			case LocaleID.CHIRP_NEED_MORE_PARKS:
			case LocaleID.CHIRP_NEW_MAP_TILE:
			case LocaleID.CHIRP_NO_ELECTRICITY:
			case LocaleID.CHIRP_NO_HEALTHCARE:
			case LocaleID.CHIRP_NO_SCHOOLS:
			case LocaleID.CHIRP_NO_WATER:
			case LocaleID.CHIRP_NOISEPOLLUTION:
			case LocaleID.CHIRP_RESIDENTIAL_DEMAND:
			case LocaleID.CHIRP_SEWAGE:
			case LocaleID.CHIRP_TRASH_PILING_UP:
			case LocaleID.CHIRP_POLLUTION:
			case LocaleID.CHIRP_POISONED:
			case LocaleID.CHIRP_LOW_HEALTH:
			case LocaleID.CHIRP_LOW_HAPPINESS:
				return false;
			default:
				return true;
			}
		}

		public string removeTwitterness(String msg){
			String message = msg;
			bool endhashesRemaining = true;
			while (endhashesRemaining){
				String endhashremoved = Regex.Replace (message, "\\s#\\w+\\s*$","");
				if (!message.Equals(endhashremoved)){
					message = endhashremoved;
				}
				else{
					endhashesRemaining = false;
				}
				
			}
			message = Regex.Replace(message, "#|@", "");
			message = Regex.Replace(message, "_", " ");
			return message;
		}
	}
}

