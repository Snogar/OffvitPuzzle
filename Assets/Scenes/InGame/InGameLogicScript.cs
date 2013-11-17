using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InGameLogicScript : MonoBehaviour {
	private const int MAX_ROW_COUNT = 8;
	private const int MAX_COL_COUNT = 8;
	private const int BLOW_MINIMUM_COUNT = 3;
	
	private TileScript[,] mTiles = new TileScript[MAX_ROW_COUNT, MAX_COL_COUNT];
	private bool mIsSwapEnable, mIsBlownThisTurn, mIsReSwapNeeded, mIsEnemyActionDone;
	private TileScript mClickedTile;
	private TileScript[] mLastSwappedTiles = new TileScript[2];
	private int mTurn;
	private int mMovingCount;
	
	private static InGameLogicScript instance;
	public static InGameLogicScript Instance {
		get { return instance; }
	}
	private void Awake() {
		instance = this;
	}
	
	// Use this for initialization
	private void Start () {
		int i, j;
		mClickedTile = null;
		for(i=0;i<MAX_ROW_COUNT;i++) {
			for(j=0;j<MAX_COL_COUNT;j++) {
				GameObject go = new GameObject("tile(" + i + "," + j + ")");
				TileScript tileScript = (TileScript)go.AddComponent("TileScript");
				tileScript.Init(i, j, new TileStatus());
				
				mTiles[i, j] = tileScript;
			}
		}
		
		BlownUpStatus[,] blownUpStatusTemp = new BlownUpStatus[MAX_ROW_COUNT, MAX_COL_COUNT];
		BlownUpStatus.Construct(blownUpStatusTemp);
		
		while(CheckBlowUpTiles(blownUpStatusTemp)) {
			for(i=0;i<MAX_ROW_COUNT;i++) {
				for(j=0;j<MAX_COL_COUNT;j++) {
					mTiles[i, j].SetTile(new TileStatus());
				}
			}
		}
		
		mIsSwapEnable = false;
		mIsReSwapNeeded = false;
		mIsEnemyActionDone = false;
		mIsBlownThisTurn = false;
		mTurn = 0;
		mMovingCount = 0;
		
		InGameUIManager.Instance.UpdateHP(UserManager.Instance.HP);
		InGameUIManager.Instance.UpdateTurn(mTurn);
	}
	
	// Update is called once per frame
	private void Update () {
		if(mIsSwapEnable && Input.GetButtonDown ("Fire1")) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit = new RaycastHit();
			
			if(Physics.Raycast(ray, out hit)) {
				foreach(TileScript tile in mTiles) {
					if(tile.Sprite.transform == hit.transform) {
						TileClicked(tile);
						break;
					}
				}
			}
		}
		
		if(!mIsSwapEnable) {
			bool isBlown = BlowUpTiles();
			mIsReSwapNeeded = mIsReSwapNeeded & !isBlown;
			mIsBlownThisTurn = mIsBlownThisTurn | isBlown;
			
			int i,j;
			for(i=0;i<MAX_ROW_COUNT;i++) {
				for(j=0;j<MAX_COL_COUNT;j++) {
					if(!mTiles[i, j].IsBlowable) break;
				}
				if(j!=MAX_COL_COUNT) break;
			}
			if(i == MAX_ROW_COUNT && !isBlown) {
				if(mIsReSwapNeeded) {
					mIsReSwapNeeded = false;
					SwapTiles(mLastSwappedTiles[0], mLastSwappedTiles[1]);
				}
				else if(!mIsEnemyActionDone) {
					EnemyAction();
				}
				else {
					TurnEnd();
				}
			}
		}
	}
	
	private void TileClicked(TileScript tile) {
		//tile clicked
		Debug.Log (tile.Row + " " + tile.Col + " type " + tile.Status.Type + " color " + tile.Status.Color + " countToDestroy " + tile.Status.CountToDestroy);
		if(mClickedTile == null) {
			//first tile clicked
			mClickedTile = tile;
		}else {
			//second tile clicked
			Debug.Log (System.Math.Abs(mClickedTile.Row - tile.Row) + System.Math.Abs (mClickedTile.Col - tile.Col));
			if(System.Math.Abs(mClickedTile.Row - tile.Row) + System.Math.Abs (mClickedTile.Col - tile.Col) != 1) {
				Debug.Log ("NOT POSSIBLE TO SWAP");
				mClickedTile = tile;
				return;
			}							
			
			//swap tiles
			SwapTiles(mClickedTile, tile);
			mIsReSwapNeeded = true;
			mIsSwapEnable = false;
			mIsEnemyActionDone = false;
			mIsBlownThisTurn = false;
			
			//remove click
			mClickedTile = null;
		}
	}
	
	private void SwapTiles(TileScript tileClicked, TileScript tileSwapped) {
		Vector3 tileClickedVector = tileClicked.GetTileVector(), tileSwappedVector = tileSwapped.GetTileVector();
		TileStatus tileClickedStatus = tileClicked.Status;
		
		MoveTile(tileSwappedVector, tileSwapped.Status, tileClicked);
		MoveTile(tileClickedVector, tileClickedStatus, tileSwapped);

		mLastSwappedTiles[0] = tileClicked;
		mLastSwappedTiles[1] = tileSwapped;
	}
	
	private void MoveTile(TileScript moveTile, TileScript destinationTile) {
		
		moveTile.Status.MoveTime = mMovingCount;
		mMovingCount ++;
		Debug.Log ("Moving Count = " + mMovingCount.ToString());
		destinationTile.IsBlowable = false;
		destinationTile.SetPosition(moveTile.GetTileVector());
		destinationTile.SetTile(moveTile.Status);
		StartCoroutine(InGameAnimationManager.Instance.MoveAnimation(destinationTile, destinationTile.GetTileVector()));
	}
	
	private void MoveTile(Vector3 startPosition, TileStatus tileStatus, TileScript destinationTile) {
		
		tileStatus.MoveTime = mMovingCount;
		mMovingCount ++;
		Debug.Log ("Moving Count = " + mMovingCount.ToString ());
		destinationTile.IsBlowable = false;
		destinationTile.SetPosition(startPosition);
		destinationTile.SetTile(tileStatus);
		StartCoroutine(InGameAnimationManager.Instance.MoveAnimation(destinationTile, destinationTile.GetTileVector()));
	}

	private bool CheckBlowUpTiles(BlownUpStatus [,] blownUpStatus) {
		int i, j, k;
		bool isBlown = false;
		int serialCount = 0;
		TileTypeManager.TileColor startColor = mTiles[0,0].Status.Color;
		//Right
		for(i=0;i<MAX_ROW_COUNT;i++) {
			serialCount = 0;
			for(j=0;j<MAX_COL_COUNT;j++) {
				if(!mTiles[i,j].IsBlowable) {
					serialCount = 0;
					continue;
				}
				if(serialCount == 0 || startColor != mTiles[i,j].Status.Color) {
					startColor = mTiles[i,j].Status.Color;
					serialCount = 1;
				}else{
					serialCount++;	
				}
				if(serialCount == BLOW_MINIMUM_COUNT){
					for(k = 1; k < BLOW_MINIMUM_COUNT; k++) {
						blownUpStatus[i,j-k].Horizontal = true;
					}
					isBlown = true;
				}
				if(serialCount >= BLOW_MINIMUM_COUNT){
					blownUpStatus[i,j].Horizontal = true;
				}
			}
		}
		//Down
		for(j=0;j<MAX_COL_COUNT;j++) {
			serialCount = 0;
			for(i=0;i<MAX_ROW_COUNT;i++) {
				if(!mTiles[i,j].IsBlowable) {
					serialCount = 0;
					continue;
				}
				if(serialCount == 0 || startColor != mTiles[i,j].Status.Color) {
					startColor = mTiles[i,j].Status.Color;
					serialCount = 1;
				}else{
					serialCount++;	
				}
				if(serialCount == BLOW_MINIMUM_COUNT){
					for(k = 1; k < BLOW_MINIMUM_COUNT; k++) {
						blownUpStatus[i-k,j].Horizontal = true;
					}
					isBlown = true;
				}
				if(serialCount >= BLOW_MINIMUM_COUNT){
					blownUpStatus[i,j].Horizontal = true;
				}
			}
		}
		
		int[][] fiveRightIndex = new int[5][];
		fiveRightIndex[0] = new int[2]; fiveRightIndex[0][0] = 0; fiveRightIndex[0][1] = 0;
		fiveRightIndex[1] = new int[2]; fiveRightIndex[1][0] = 0; fiveRightIndex[1][1] = 1;
		fiveRightIndex[2] = new int[2]; fiveRightIndex[2][0] = 0; fiveRightIndex[2][1] = 2;
		fiveRightIndex[3] = new int[2]; fiveRightIndex[3][0] = 0; fiveRightIndex[3][1] = 3;
		fiveRightIndex[4] = new int[2]; fiveRightIndex[4][0] = 0; fiveRightIndex[4][1] = 4;
		
		int[][] fiveDownIndex = new int[5][];
		fiveDownIndex[0] = new int[2]; fiveDownIndex[0][0] = 0; fiveDownIndex[0][1] = 0;
		fiveDownIndex[1] = new int[2]; fiveDownIndex[1][0] = 1; fiveDownIndex[1][1] = 0;
		fiveDownIndex[2] = new int[2]; fiveDownIndex[2][0] = 2; fiveDownIndex[2][1] = 0;
		fiveDownIndex[3] = new int[2]; fiveDownIndex[3][0] = 3; fiveDownIndex[3][1] = 0;
		fiveDownIndex[4] = new int[2]; fiveDownIndex[4][0] = 4; fiveDownIndex[4][1] = 0;

		//five 
		for(i=0;i<MAX_ROW_COUNT;i++) {
			for(j=0;j<MAX_COL_COUNT;j++) {
				// Horizontal
				if(TileScript.HasSameColor(mTiles, i, j, fiveRightIndex)){
					BlownUpStatus.SetShape(blownUpStatus, i, j, fiveRightIndex, BlownUpStatus.EffectShape.FIVE);
				}
				// Vertical
				if(TileScript.HasSameColor(mTiles, i, j, fiveDownIndex)){
					BlownUpStatus.SetShape(blownUpStatus, i, j, fiveDownIndex, BlownUpStatus.EffectShape.FIVE);
				}
			}
		}
		
		int[][] L1Index = new int[5][];
		L1Index[0] = new int[2]; L1Index[0][0] = 0; L1Index[0][1] = 0;
		L1Index[1] = new int[2]; L1Index[1][0] = 0; L1Index[1][1] = -1;
		L1Index[2] = new int[2]; L1Index[2][0] = 0; L1Index[2][1] = -2;
		L1Index[3] = new int[2]; L1Index[3][0] = 1; L1Index[3][1] = 0;
		L1Index[4] = new int[2]; L1Index[4][0] = 2; L1Index[4][1] = 0;
		
		int[][] L2Index = new int[5][];
		L2Index[0] = new int[2]; L2Index[0][0] = 0; L2Index[0][1] = 0;
		L2Index[1] = new int[2]; L2Index[1][0] = 0; L2Index[1][1] = -1;
		L2Index[2] = new int[2]; L2Index[2][0] = 0; L2Index[2][1] = -2;
		L2Index[3] = new int[2]; L2Index[3][0] = -1; L2Index[3][1] = 0;
		L2Index[4] = new int[2]; L2Index[4][0] = -2; L2Index[4][1] = 0;
		
		int[][] L3Index = new int[5][];
		L3Index[0] = new int[2]; L3Index[0][0] = 0; L3Index[0][1] = 0;
		L3Index[1] = new int[2]; L3Index[1][0] = 0; L3Index[1][1] = 1;
		L3Index[2] = new int[2]; L3Index[2][0] = 0; L3Index[2][1] = 2;
		L3Index[3] = new int[2]; L3Index[3][0] = -1; L3Index[3][1] = 0;
		L3Index[4] = new int[2]; L3Index[4][0] = -2; L3Index[4][1] = 0;
		
		int[][] L4Index = new int[5][];
		L4Index[0] = new int[2]; L4Index[0][0] = 0; L4Index[0][1] = 0;
		L4Index[1] = new int[2]; L4Index[1][0] = 0; L4Index[1][1] = 1;
		L4Index[2] = new int[2]; L4Index[2][0] = 0; L4Index[2][1] = 2;
		L4Index[3] = new int[2]; L4Index[3][0] = 1; L4Index[3][1] = 0;
		L4Index[4] = new int[2]; L4Index[4][0] = 2; L4Index[4][1] = 0;
		
		//Giyeok
		for(i=0;i<MAX_ROW_COUNT;i++){
			for(j=0;j<MAX_COL_COUNT;j++){
				// Giyeok and right rotation
				//first Giyeok
				if(TileScript.HasSameColor(mTiles, i, j, L1Index)){
					BlownUpStatus.SetShape(blownUpStatus, i, j, L1Index, BlownUpStatus.EffectShape.L);
				}
				//second Giyeok
				if(TileScript.HasSameColor(mTiles, i, j, L2Index)){
					BlownUpStatus.SetShape(blownUpStatus, i, j, L2Index, BlownUpStatus.EffectShape.L);
				}
				// third Giyeok
				if(TileScript.HasSameColor(mTiles, i, j, L3Index)){
					BlownUpStatus.SetShape(blownUpStatus, i, j, L3Index, BlownUpStatus.EffectShape.L);
				}
				//fourth Giyeok
				if(TileScript.HasSameColor(mTiles, i, j, L4Index)){
					BlownUpStatus.SetShape(blownUpStatus, i, j, L4Index, BlownUpStatus.EffectShape.L);
				}
			}
		}

		int[][] fourRightIndex = new int[4][];
		fourRightIndex[0] = new int[2]; fourRightIndex[0][0] = 0; fourRightIndex[0][1] = 0;
		fourRightIndex[1] = new int[2]; fourRightIndex[1][0] = 0; fourRightIndex[1][1] = 1;
		fourRightIndex[2] = new int[2]; fourRightIndex[2][0] = 0; fourRightIndex[2][1] = 2;
		fourRightIndex[3] = new int[2]; fourRightIndex[3][0] = 0; fourRightIndex[3][1] = 3;
		
		int[][] fourDownIndex = new int[4][];
		fourDownIndex[0] = new int[2]; fourDownIndex[0][0] = 0; fourDownIndex[0][1] = 0;
		fourDownIndex[1] = new int[2]; fourDownIndex[1][0] = 1; fourDownIndex[1][1] = 0;
		fourDownIndex[2] = new int[2]; fourDownIndex[2][0] = 2; fourDownIndex[2][1] = 0;
		fourDownIndex[3] = new int[2]; fourDownIndex[3][0] = 3; fourDownIndex[3][1] = 0;

		//Four
		for(i=0;i<MAX_ROW_COUNT;i++) {
			for(j=0;j<MAX_COL_COUNT;j++) {
				// Vertical
				if(TileScript.HasSameColor(mTiles, i, j, fourDownIndex)){
					BlownUpStatus.SetShape(blownUpStatus, i, j, fourDownIndex, BlownUpStatus.EffectShape.FOUR);
				}
				// Horizontal
				if(TileScript.HasSameColor(mTiles, i, j, fourRightIndex)){
					BlownUpStatus.SetShape(blownUpStatus, i, j, fourRightIndex, BlownUpStatus.EffectShape.FOUR);
				}
			}
		}
		return isBlown;
	}
	
	private bool BlowUpTiles() {
		int i, j;
		BlownUpStatus[,] blownUpStatus = new BlownUpStatus[MAX_ROW_COUNT, MAX_COL_COUNT];
		BlownUpStatus.Construct(blownUpStatus);
		
		bool[,] tilesDestroyed = new bool[MAX_ROW_COUNT, MAX_COL_COUNT];
		
		if(!CheckBlowUpTiles(blownUpStatus)) return false;
		
		
		
		// delete 
		for(i=0;i<MAX_ROW_COUNT;i++) {
			for(j=0;j<MAX_COL_COUNT;j++) {
				// add
				if(blownUpStatus[i,j].IsBlownUp()) {
					mTiles[i,j].Status.CountToDestroy--;
					if(mTiles[i,j].Status.CountToDestroy <= 0) {
						//Tile Destroyed :: give exp, money and special effects
						tilesDestroyed[i,j] = true;
					}
				}
			}
		}
			
		
		int row;
		for(j=0;j<MAX_COL_COUNT;j++) {
			Queue<int> q = new Queue<int>();
			for(i=MAX_ROW_COUNT-1;i>=0;i--) {
				if(tilesDestroyed[i, j]) {
					q.Enqueue(i);
				}else {
					if(q.Count > 0) {
						row = q.Dequeue();
						mTiles[row, j].IsBlowable = false;
						mTiles[row, j].SetPosition(mTiles[i,j].gameObject.transform.position);
						mTiles[row, j].SetTile(mTiles[i,j].Status);
						StartCoroutine(InGameAnimationManager.Instance.TileMoveToOriginalPositionStart(mTiles[row, j]));
						q.Enqueue(i);
					}
				}
			}
			
			while(q.Count > 0) {
				row = q.Dequeue();
				mTiles[row, j].IsBlowable = false;
				
				Vector3 topTilePosition = TileScript.GetTileVectorWithRowCol(0, j);
				if(q.Count + 1 < MAX_ROW_COUNT) {
					Vector3 lastFellingTilePosition = mTiles[q.Count+1,j].gameObject.transform.position;
					if(lastFellingTilePosition.y <= topTilePosition.y) 
						mTiles[row, j].SetPosition(new Vector3(topTilePosition.x, topTilePosition.y+TileScript.tileSize, 0));
					else 
						mTiles[row, j].SetPosition(new Vector3(lastFellingTilePosition.x, lastFellingTilePosition.y+TileScript.tileSize, 0));
				}else {
					mTiles[row, j].SetPosition(new Vector3(topTilePosition.x, topTilePosition.y+TileScript.tileSize, 0));
				}
				mTiles[row, j].SetTile(new TileStatus());
				
				StartCoroutine(InGameAnimationManager.Instance.TileMoveToOriginalPositionStart(mTiles[row, j]));
			}
		}
		return true;
	}
	
	private void EnemyAction() {
		int i, j;
		
		mIsEnemyActionDone = true;
		if(!mIsBlownThisTurn) return;
		
		for(i=MAX_ROW_COUNT-1;i>=0;i--) {
			for(j=0;j<MAX_COL_COUNT;j++) {
				if(!TileTypeManager.Instance.IsEnemyType(mTiles[i,j].Status.Type)) continue;
				if(mTiles[i, j].Status.Type == TileTypeManager.TileType.ENEMY_ARCHER) {
					if(i >= TileTypeManager.ARCHER_ATTACK_ROW - 1) EnemyAttack(i,j);
					else EnemyMove(i,j);
				}
				else if(mTiles[i,j].Status.Type == TileTypeManager.TileType.ENEMY_WIZARD) {
					if(i >= TileTypeManager.WIZARD_ATTACK_ROW - 1) EnemyAttack(i,j);
					else EnemyMove(i,j);
				}
				else {
					if(i != MAX_ROW_COUNT-1) EnemyMove(i,j);
					else EnemyAttack(i,j);
				}
				
			}
		}
	}
	
	private void EnemyMove(int row, int col) {
		int k, moveToRow = -1;
		for(k=1;k<=mTiles[row,col].Status.MovementSpeed;k++) {
			if(row+k < MAX_ROW_COUNT && TileTypeManager.Instance.IsEnemyRemovableTile(mTiles[row+k, col].Status.Type)) {
				moveToRow = row+k;
			}
		}
		
		if(moveToRow != -1) {
			TileStatus status = mTiles[row,col].Status;
			for(k=row+1;k<=moveToRow;k++) {
				MoveTile(mTiles[k,col], mTiles[k-1,col]);
			}
			MoveTile(TileScript.GetTileVectorWithRowCol(row,col), status, mTiles[moveToRow, col]);
		}
	}
	
	private void EnemyAttack(int row, int col) {
		//TODO : enemy wizard attacks skill gauge
		
		mTiles[row,col].Status.TurnLeftAttack--;
		if(mTiles[row,col].Status.TurnLeftAttack <= 0) {
			mTiles[row,col].Status.AttackTurnReset();
			mTiles[row,col].IsBlowable = false;
			UserManager.Instance.decreaseHP(TileTypeManager.Instance.GetEnemyDamage(mTiles[row, col].Status.Type));
			StartCoroutine(InGameAnimationManager.Instance.EnemyAttackActionStart(mTiles[row, col]));
		}
	}
	
	private void TurnEnd() {
		mIsSwapEnable = true;
		if(!mIsBlownThisTurn) return; //Turn is not proceeded. (irregular swap)
		
		mTurn ++;
		InGameUIManager.Instance.UpdateTurn(mTurn);
	}
}
