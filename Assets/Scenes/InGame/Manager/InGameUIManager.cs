using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InGameUIManager : MonoBehaviour 
{
	private UILabel mHPLabel, mMPLabel;
	
	private static InGameUIManager instance;
	public static InGameUIManager Instance {
		get { return instance; }
	}
	private void Awake() {
		instance = this;
		
		mHPLabel = transform.Find("GaugeUIPanel/HP").GetComponent<UILabel>();
		mMPLabel = transform.Find("GaugeUIPanel/MP").GetComponent<UILabel>();
	}
	
	public void UpdateHP(int hp) {
		mHPLabel.text = "HP : " + hp;
	}
	
	public void UpdateTurn(int turn) {
		//mTurn.text = "TURN : " + turn;
	}
	
	public void UpdateMP(int mp) {
		mMPLabel.text = "MP : " + mp;
	}
}

