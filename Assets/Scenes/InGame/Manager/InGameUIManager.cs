using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InGameUIManager : MonoBehaviour 
{
	private UILabel mHP;
	private UILabel mTurn;
	
	private static InGameUIManager instance;
	public static InGameUIManager Instance {
		get { return instance; }
	}
	private void Awake() {
		instance = this;
		
		mHP = transform.Find("HP").GetComponent<UILabel>();
		mTurn = transform.Find("Turn").GetComponent<UILabel>();
	}
	
	private void Start () {
	}

	private void Update () {
	}
	
	public void UpdateHP(int hp) {
		mHP.text = "HP : " + hp;
	}
	
	public void UpdateTurn(int turn) {
		mTurn.text = "TURN : " + turn;
	}
}

