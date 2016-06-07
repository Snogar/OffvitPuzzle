using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InGameAnimationManager : MonoBehaviour {
	private const float SWAP_ANIMATION_TIME = 0.3f;
	private const float TILE_FELL_SPEED = 384.0f;
	private const float TILE_BOUNCE_SPEED = 64.0f;
	private const float TILE_DESTROY_DELAY = 0.0f;
	private const float ENEMY_ATTACK_ACTION_SCALE = 1.5f;
	private const float ENEMY_ATTACK_ACTION_SCALE_TIME = 0.3f;
	private const float ENEMY_ATTACK_ACTION_DELAY_TIME = 0.3f;
	
	private static InGameAnimationManager instance;
	public static InGameAnimationManager Instance {
		get { return instance; }
	}
	private void Awake() {
		instance = this;
	}
	
	//iTween.Stop needs some time to stop before starting action
	public void resetTileAction(TileScript tile) {
		iTween.Stop(tile.gameObject);
		tile.gameObject.transform.localScale = TileTypeManager.Instance.GetTileScale(tile.Status.Type);
	}
	
	public void onCompleteTileAction(TileScript tile) {
		tile.IsBlowable = true;
	}
	
	public IEnumerator MoveAnimation(TileScript tile, Vector3 destination) {
		//resetTileAction(tile);
		//yield return new WaitForSeconds(0.05f);
		iTween.MoveTo(tile.gameObject, iTween.Hash("isLocal", true, "x", destination.x, "y", destination.y, "easeType", "easeInOutSine", "time", SWAP_ANIMATION_TIME, "onComplete", "onCompleteTileAction", "onCompleteTarget", gameObject, "onCompleteParams", tile));
		yield return null;
	}
	
	public IEnumerator TileMoveToOriginalPositionStart(TileScript tile) {
		resetTileAction(tile);
		yield return new WaitForSeconds(0.05f);
		
		Queue<iTweenChainManager.iTweenChainParameter> methodQueue = new Queue<iTweenChainManager.iTweenChainParameter>();
		methodQueue.Enqueue(iTweenChainManager.Parameter("MoveTo", tile.gameObject, new Hashtable() {
			{"isLocal", true},
			{"x", tile.GetTileVector().x},
			{"y", tile.GetTileVector().y - TileScript.tileSize/10},
			{"speed", TILE_FELL_SPEED}, 
			{"delay", TILE_DESTROY_DELAY},
			{"easetype", "easeOutQuad"}
		}));
		methodQueue.Enqueue(iTweenChainManager.Parameter("MoveTo", tile.gameObject, new Hashtable() {
			{"isLocal", true},
			{"x", tile.GetTileVector().x}, 
			{"y", tile.GetTileVector().y}, 
			{"speed", TILE_BOUNCE_SPEED}, 
			{"easetype", "easeOutQuad"},
			{"onComplete", "onCompleteTileAction"}, 
			{"onCompleteTarget", gameObject}, 
			{"onCompleteParams", tile}
		}));
		iTweenChainManager.instance.Execute(methodQueue);	 
	}
	
	public IEnumerator EnemyAttackActionStart(TileScript tile) {
		resetTileAction(tile);
		yield return new WaitForSeconds(0.05f);
		
		Vector3 origScale = TileTypeManager.Instance.GetTileScale(tile.Status.Type);
		Queue<iTweenChainManager.iTweenChainParameter> methodQueue = new Queue<iTweenChainManager.iTweenChainParameter>();
		methodQueue.Enqueue(iTweenChainManager.Parameter("ScaleBy", tile.gameObject, new Hashtable() {
			{"isLocal", true},
			{"x", ENEMY_ATTACK_ACTION_SCALE},
			{"y", ENEMY_ATTACK_ACTION_SCALE},
			{"time", ENEMY_ATTACK_ACTION_SCALE_TIME}, 
			{"easetype", "linear"}
		}));
		methodQueue.Enqueue(iTweenChainManager.Parameter("ScaleTo", tile.gameObject, new Hashtable() {
			{"isLocal", true},
			{"x", origScale.x}, 
			{"y", origScale.y}, 
			{"delay", ENEMY_ATTACK_ACTION_DELAY_TIME},
			{"time", ENEMY_ATTACK_ACTION_SCALE_TIME}, 
			{"easetype", "linear"},
			{"onComplete", "EnemyAttackActionEnd"}, 
			{"onCompleteTarget", gameObject}, 
			{"onCompleteParams", tile}
		}));
		iTweenChainManager.instance.Execute(methodQueue);	 
	}
	
	public void EnemyAttackActionEnd(TileScript tile) {
		onCompleteTileAction(tile);
		InGameUIManager.Instance.UpdateHP(UserManager.Instance.HP);
	}
}
