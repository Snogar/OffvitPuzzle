using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class iTweenChainManager : MonoBehaviour{
	/*
	 * Using Example : 
	 	Queue<iTweenChainManager.iTweenChainParameter> methodQueue = new Queue<iTweenChainManager.iTweenChainParameter>();
		methodQueue.Enqueue(iTweenChainManager.Parameter("MoveTo", kudo, new Hashtable() {
			{"isLocal", true},
			{"y", pos.y - KUDO_UI_HEIGHT},
			{"time", KUDO_APPEARING_TIME},
			{"easetype", iTween.EaseType.easeOutSine}
		}));
		methodQueue.Enqueue(iTweenChainManager.Parameter("MoveTo", kudo, new Hashtable() {
			{"isLocal", true},
			{"y", pos.y}, 
			{"delay", KUDO_SHOWING_TIME},
			{"time", KUDO_DISAPPEARING_TIME}, 
			{"easetype", iTween.EaseType.easeOutSine},
			{"onComplete", "ShowKudoAnimationComplete"}, 
			{"onCompleteTarget", gameObject}, 
			{"onCompleteParams", kudo}
		}));
		
		iTweenChainManager.instance.Execute(methodQueue);	 
	 *
	 */
	
	private static iTweenChainManager instance_ = null;
	public static iTweenChainManager instance {
		get { 
			if(instance_ == null) {
				GameObject go = new GameObject("iTweenChainManager");
				instance_ = go.AddComponent<iTweenChainManager>();
			}
			return instance_; 
		} 
	}
	
	public class iTweenChainParameter {
		public string methodName;
		public GameObject targetGameObject;
		public Hashtable hashtable;
		
		public iTweenChainParameter (string methodName, GameObject targetGameObject, Hashtable hashtable) {
			this.methodName = methodName;
			this.targetGameObject = targetGameObject;
			this.hashtable = hashtable;
		}
	}
	
	public static iTweenChainParameter Parameter(string methodName, GameObject targetGameObject, Hashtable hashtable) {
		return new iTweenChainParameter(methodName, targetGameObject, hashtable);
	}
	
	public void Execute(Queue<iTweenChainParameter> methodQueue) {
		if(methodQueue.Count > 0) {
			iTweenChainParameter param = methodQueue.Peek();
			param.hashtable = ConvertHashKeyLowercase(param.hashtable);
			Hashtable modifiedHashtable = new Hashtable(param.hashtable);
			modifiedHashtable["oncomplete"] = "ExecuteInProgress";
			modifiedHashtable["oncompletetarget"] = gameObject;
			modifiedHashtable["oncompleteparams"] = methodQueue;
			System.Type.GetType("iTween").GetMethod(param.methodName, new[]{typeof(GameObject), typeof(Hashtable)}).Invoke(null, new object[]{param.targetGameObject, modifiedHashtable});
		}
	}
	
	private void ExecuteInProgress(Queue<iTweenChainParameter> methodQueue) {
		iTweenChainParameter param = methodQueue.Dequeue();
		if(param.hashtable.ContainsKey("oncomplete")) {
			GameObject target = param.targetGameObject;
			if(param.hashtable.ContainsKey("oncompletetarget")) target = (GameObject)param.hashtable["oncompletetarget"];
			target.SendMessage((string)param.hashtable["oncomplete"], (object)param.hashtable["oncompleteparams"], SendMessageOptions.DontRequireReceiver);
		}
		Execute(methodQueue);
	}
				
	private Hashtable ConvertHashKeyLowercase(Hashtable hashtable) {
		Hashtable tempHashtable = new Hashtable();
		foreach (string key in hashtable.Keys) {
			string lowerKey = key.ToLower();
			tempHashtable.Add(lowerKey, hashtable[key]);
		}
		return tempHashtable;
	}
}

