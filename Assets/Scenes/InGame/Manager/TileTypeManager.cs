using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileTypeManager {	
	public enum TileColor {
		BLUE,
		GREEN,
		WHITE,
		PINK,
		PURPLE,
		MAX_COUNT
	}
	public enum TileType {
		NORMAL,
		HEAL,
		CROSS,
		SPECIAL,
		ENEMY_WARRIOR,
		ENEMY_ARCHER,
		ENEMY_GIANT,
		ENEMY_ASSASSIN,
		ENEMY_WIZARD,
		//ENEMY_PROTECTOR (TAUNT)
		//ENEMY_BUFFER (GIVES TITLE - BUFFS DAMAGE, SPEED, HP, ETC...)
		MAX_COUNT
	}
	public const int ARCHER_ATTACK_ROW = 5;
	public const int WIZARD_ATTACK_ROW = 5;
	
	private string[,] tileSpriteName = {
		//TODO: HEAL AND CROSS
		//NORMAL	 	 HEAL	   		CROSS	  		SPECIAL			ENEMY_WARR 			ENEMY_ARCH 		 ENEMY_GIANT       ENEMY_ASSA        ENEMY_WIZ
		{"blue",	"blue_buffer",	"blue_assassin",	"blue_extra",   "blue_warrior",		"blue_archer",   "blue_giant",   "blue_assassin",   "blue_wizard"},
		{"green",	"green_buffer",	"green_assassin",	"green_extra",	"green_warrior",	"green_archer",  "green_giant",  "green_assassin",  "green_wizard"},
		{"white",	"white_buffer", "white_assassin",	"white_extra",	"white_warrior",	"white_archer",  "white_giant",  "white_assassin",  "white_wizard"},
		{"pink",	"pink_buffer", 	"pink_assassin",	"pink_extra",   "pink_warrior",		"pink_archer",   "pink_giant",   "pink_assassin",   "pink_wizard"},
		{"purple",	"purple_buffer","purple_assassin",	"purple_extra",	"purple_warrior",	"purple_archer", "purple_giant", "purple_assassin", "purple_wizard"},
	};
	
	private static TileTypeManager instance;
	public static TileTypeManager Instance {
		get { 
			if(instance == null) {
				instance = new TileTypeManager();
			}
			return instance; 
		}
	}
	
	public string SpriteName(TileType tileType, TileColor tileColor) {
		return tileSpriteName[(int)tileColor, (int)tileType];
	}
	
	public bool IsEnemyType(TileType tileType) {
		if(tileType == TileType.ENEMY_WARRIOR) return true;
		else if(tileType == TileType.ENEMY_ARCHER) return true;
		else if(tileType == TileType.ENEMY_GIANT) return true;
		else if(tileType == TileType.ENEMY_ASSASSIN) return true;
		else if(tileType == TileType.ENEMY_WIZARD) return true;
		return false;
	}
	
	public bool IsEnemyRemovableTile(TileType tileType) {
		return !IsEnemyType(tileType);
	}
	
	public int GetEnemyDamage(TileType tileType) {
		if(tileType == TileType.ENEMY_WARRIOR) return 2;
		else if(tileType == TileType.ENEMY_ARCHER) return 1;
		else if(tileType == TileType.ENEMY_GIANT) return 3;
		else if(tileType == TileType.ENEMY_ASSASSIN) return 1;
		else if(tileType == TileType.ENEMY_WIZARD) return 1;
		return 0;
	}
	
	public int GetCountToDestroy(TileType tileType) {
		if(tileType == TileType.ENEMY_GIANT) return 2;
		return 1;
	}
	
	public int GetMovementSpeed(TileType tileType) {
		if(!IsEnemyType(tileType)) return 0;
		else if(tileType == TileType.ENEMY_ASSASSIN) return 2;
		return 1;
	}
	
	public int GetAttackSpeed(TileType tileType) {
		if(!IsEnemyType(tileType)) return 0;
		else if(tileType == TileType.ENEMY_GIANT) return 2;
		return 1;
	}
	
	public Vector3 GetTileScale(TileType tileType) {
		if(tileType == TileType.ENEMY_GIANT) return new Vector3(1.3f, 1.3f, 1.0f);
		return new Vector3(1, 1, 1);
	}
}