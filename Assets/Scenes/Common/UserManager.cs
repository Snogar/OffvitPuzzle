using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UserManager {	
	private const int MP_INCREASING_CONSTANT = 3;
	private int mHP;
	private int mMP;
	
	public UserManager() {
		mHP = 50;
		mMP = 0;
	}
	
	private static UserManager instance;
	public static UserManager Instance {
		get { 
			if(instance == null) {
				instance = new UserManager();
			}
			return instance; 
		}
	}
	
	public int HP {
		get { return mHP; }
	}
	
	public int MP {
		get { return mMP; }
	}
	
	public int decreaseHP(int decreaseAmount) {
		mHP -= decreaseAmount;
		return mHP;
	}

	public int setMP(int mp) {
		mMP = mp;
		return mMP;
	}
}