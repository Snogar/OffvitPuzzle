using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InGameUIManager : MonoBehaviour 
{
	private tk2dTextMesh mHP;
	private tk2dTextMesh mTurn;
	
	private static InGameUIManager instance;
	public static InGameUIManager Instance {
		get { return instance; }
	}
	private void Awake() {
		instance = this;
		
		mHP = transform.Find("HP").GetComponent<tk2dTextMesh>();
		mTurn = transform.Find("Turn").GetComponent<tk2dTextMesh>();
	}
	
	private void Start () {
	}

	private void Update () {
	}
	
	public void UpdateHP(int hp) {
		mHP.text = "HP : " + hp;
		mHP.Commit();
	}
	
	public void UpdateTurn(int turn) {
		mTurn.text = "TURN : " + turn;
		mTurn.Commit();
	}
}

