using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UserManager {	
	private int mHP;
	
	public UserManager() {
		mHP = 50;
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
	
	public int decreaseHP(int decreaseAmount) {
		mHP -= decreaseAmount;
		return mHP;
	}
}