using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileTypeManager {	
	public enum TileColor {
		BLUE,
		GREEN,
		YELLOW,
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
		{"Blue",	"Blue_Buffer",	"Blue_Assassin",	"Blue_Extra",   "Blue_Warrior",		"Blue_Archer",   "Blue_Giant",   "Blue_Assassin",   "Blue_Wizard"},
		{"Green",	"Green_Buffer",	"Green_Assassin",	"Green_Extra",	"Green_Warrior",	"Green_Archer",  "Green_Giant",  "Green_Assassin",  "Green_Wizard"},
		{"Yellow",	"Yellow_Buffer","Yellow_Assassin",	"Yellow_Extra",	"Yellow_Warrior",	"Yellow_Archer", "Yellow_Giant", "Yellow_Assassin", "Yellow_Wizard"},
		{"Pink",	"Pink_Buffer", 	"Pink_Assassin",	"Pink_Extra",   "Pink_Warrior",		"Pink_Archer",   "Pink_Giant",   "Pink_Assassin",   "Pink_Wizard"},
		{"Purple",	"Purple_Buffer","Purple_Assassin",	"Purple_Extra",	"Purple_Warrior",	"Purple_Archer", "Purple_Giant", "Purple_Assassin", "Purple_Wizard"},
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