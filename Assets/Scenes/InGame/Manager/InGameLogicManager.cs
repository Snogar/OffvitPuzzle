using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InGameLogicManager : MonoBehaviour {
	private const int MAX_ROW_COUNT = 8;
	private const int MAX_COL_COUNT = 8;
	private const int BLOW_MINIMUM_COUNT = 3;
	private const int MP_INCREASING_CONSTANT = 3;
	
	private TileScript[,] mTiles = new TileScript[MAX_ROW_COUNT, MAX_COL_COUNT];
	private bool mIsSwapEnable, mIsBlownThisTurn, mIsReSwapNeeded, mIsEnemyActionDone;
	private TileScript mClickedTile;
	private TileScript[] mLastSwappedTiles = new TileScript[2];
	private int mTurn;
	private int mBlownTileCount;
	private int mBaseMP;
	
	private bool[,] mCheckToDestroyed;
	private bool[,] mTilesDestroyed;
	
	private static InGameLogicManager instance;
	public static InGameLogicManager Instance {
		get { return instance; }
	}
	private void Awake() {
		instance = this;
	}
	
	// Use this for initialization
	private void Start () {
		int i, j;
		GameObject tileObject = Resources.Load("InGame/Tile", typeof(GameObject)) as GameObject;
		
		for(i=0;i<MAX_ROW_COUNT;i++) {
			for(j=0;j<MAX_COL_COUNT;j++) {
				GameObject tileObjectClone = (GameObject)Instantiate(tileObject);
				tileObjectClone.name = "Tile(" + i + "," + j + ")";
				TileScript tileScript = tileObjectClone.GetComponent<TileScript>();
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
		
		mClickedTile = null;
		mIsSwapEnable = false;
		mIsReSwapNeeded = false;
		mIsEnemyActionDone = false;
		mIsBlownThisTurn = false;
		mTurn = 0;
		mBlownTileCount = 0;
		mBaseMP = 0;
		
		InGameUIManager.Instance.UpdateHP(UserManager.Instance.HP);
		InGameUIManager.Instance.UpdateMP(UserManager.Instance.MP);
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
			mBaseMP = UserManager.Instance.MP;
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
		moveTile.Status.SetMoveTime();
		destinationTile.IsBlowable = false;
		destinationTile.SetPosition(moveTile.GetTileVector());
		destinationTile.SetTile(moveTile.Status);
		StartCoroutine(InGameAnimationManager.Instance.MoveAnimation(destinationTile, destinationTile.GetTileVector()));
	}
	
	private void MoveTile(Vector3 startPosition, TileStatus tileStatus, TileScript destinationTile) {
		tileStatus.SetMoveTime();
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
						blownUpStatus[i,j-k].mHorizontal = true;
						blownUpStatus[i,j-k].mShape = BlownUpStatus.EffectShape.NONE;
					}
					isBlown = true;
				}
				if(serialCount >= BLOW_MINIMUM_COUNT){
					blownUpStatus[i,j].mHorizontal = true;
					blownUpStatus[i,j].mShape = BlownUpStatus.EffectShape.NONE;
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
						blownUpStatus[i-k,j].mVertical = true;
						blownUpStatus[i-k,j].mShape = BlownUpStatus.EffectShape.NONE;
					}
					isBlown = true;
				}
				if(serialCount >= BLOW_MINIMUM_COUNT){
					blownUpStatus[i,j].mVertical = true;
					blownUpStatus[i,j].mShape = BlownUpStatus.EffectShape.NONE;
				}
			}
		}
		
		int[,] fiveRightIndex = new int[5,2] {{0, 0}, {0, 1}, {0, 2}, {0, 3}, {0, 4}};
		int[,] fiveDownIndex  = new int[5,2] {{0, 0}, {1, 0}, {2, 0}, {3, 0}, {4, 0}};

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
		
		int[,] L1Index = new int[5,2] {{0, 0}, {0, -1}, {0, -2}, {1,  0}, {2,  0}};
		int[,] L2Index = new int[5,2] {{0, 0}, {0, -1}, {0, -2}, {-1, 0}, {-2, 0}};
		int[,] L3Index = new int[5,2] {{0, 0}, {0,  1}, {0,  2}, {-1, 0}, {-2, 0}};
		int[,] L4Index = new int[5,2] {{0, 0}, {0,  1}, {0,  2}, {1,  0}, {2,  0}};
		
		int[,] T1Index = new int[5,2] {{0, 0}, {0,  1}, {0, -1}, {1,  0}, {2,  0}};
		int[,] T2Index = new int[5,2] {{0, 0}, {0,  1}, {0, -1}, {-1, 0}, {-2, 0}};
		int[,] T3Index = new int[5,2] {{0, 0}, {0, -1}, {0, -2}, {-1, 0}, {1,  0}};
		int[,] T4Index = new int[5,2] {{0, 0}, {0,  1}, {0,  2}, {-1, 0}, {1,  0}};
		
		int[,] plusIndex = new int[5,2] {{0, 0}, {0,  1}, {0,  -1}, {-1, 0}, {1,  0}};
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
				
				//first T-shape
				if(TileScript.HasSameColor(mTiles, i, j, T1Index)){
					BlownUpStatus.SetShape(blownUpStatus, i, j, T1Index, BlownUpStatus.EffectShape.L);
				}
				//second T-shape
				if(TileScript.HasSameColor(mTiles, i, j, T2Index)){
					BlownUpStatus.SetShape(blownUpStatus, i, j, T2Index, BlownUpStatus.EffectShape.L);
				}
				//third T-shape
				if(TileScript.HasSameColor(mTiles, i, j, T3Index)){
					BlownUpStatus.SetShape(blownUpStatus, i, j, T3Index, BlownUpStatus.EffectShape.L);
				}
				//fourth T-shape
				if(TileScript.HasSameColor(mTiles, i, j, T4Index)){
					BlownUpStatus.SetShape(blownUpStatus, i, j, T4Index, BlownUpStatus.EffectShape.L);
				}
				
				//plus-shpae
				if(TileScript.HasSameColor(mTiles, i, j, plusIndex)){
					BlownUpStatus.SetShape(blownUpStatus, i, j, plusIndex, BlownUpStatus.EffectShape.L);
				}
			}
		}
		
		int[,] fourRightIndex = new int[4,2] {{0, 0}, {0, 1}, {0, 2}, {0, 3}};
		int[,] fourDownIndex  = new int[4,2] {{0, 0}, {1, 0}, {2, 0}, {3, 0}};

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
		
		//None
		return isBlown;
	}
	private int[] SearchSameShape(BlownUpStatus[,] blownUpStatus,int start_row,int start_col){
		BlownUpStatus.EffectShape shape = blownUpStatus[start_row,start_col].mShape;
		Queue<int[]> q = new Queue<int[]>();
		int[] maxPath = new int[2] {-1,-1};
		int maxMovingTime = -1;
		q.Enqueue((new int[2] {start_row,start_col}));
		int[] now;
		int[,] index = new int[4,2] {{0,1},{0,-1},{1,0},{-1,0}};
		int start,end;
		while(q.Count != 0){
			now = q.Dequeue();
			mTilesDestroyed[now[0],now[1]] = true;
			if(mTiles[now[0],now[1]].Status.MoveTime > maxMovingTime){
				if(mTiles[now[0],now[1]].Status.CountToDestroy == 1){
					maxMovingTime = mTiles[now[0],now[1]].Status.MoveTime;
					maxPath[0] = now[0];
					maxPath[1] = now[1];
				}
			}
			start = blownUpStatus[now[0],now[1]].mHorizontal? 0:2;
			end = blownUpStatus[now[0],now[1]].mVertical? 4:2;
			for(int i=start;i<end;i++){
				if(SearchSameShapeIs(blownUpStatus,now[0]+index[i,0],now[1]+index[i,1],shape) && !mCheckToDestroyed[now[0]+index[i,0],now[1]+index[i,1]]){
					mCheckToDestroyed[now[0]+index[i,0],now[1]+index[i,1]] = true;
					q.Enqueue((new int[2] {now[0]+index[i,0],now[1]+index[i,1]}));
				}
			}
		}
		return maxPath;
	}
	private bool SearchSameShapeIs(BlownUpStatus[,] blownUpStatus,int now_row,int now_col,BlownUpStatus.EffectShape shape){
		if(now_row < 0 || now_row >= MAX_ROW_COUNT || now_col < 0 || now_col >=MAX_COL_COUNT) return false;
		if(blownUpStatus[now_row, now_col].mShape != shape) return false;
		return true;
	}
	private void MakeBubble(){
		int i,j;
		for(i=0;i<MAX_ROW_COUNT;i++){
			for(j=0;j<MAX_COL_COUNT;j++){
				if(TileTypeManager.Instance.IsEnemyType(mTiles[i,j].Status.Type)){
					mTiles[i,j].Status.SetBubbleCount();
				}
			}
		}
	}
	private bool BlowUpTiles() {
		int i, j;
		BlownUpStatus[,] blownUpStatus = new BlownUpStatus[MAX_ROW_COUNT, MAX_COL_COUNT];
		BlownUpStatus.Construct(blownUpStatus);
		
		mCheckToDestroyed = new bool[MAX_ROW_COUNT,MAX_COL_COUNT];
		mTilesDestroyed = new bool[MAX_ROW_COUNT, MAX_COL_COUNT];
		if(!CheckBlowUpTiles(blownUpStatus)) return false;
		// delete
		for(i=0;i<MAX_ROW_COUNT;i++) {
			for(j=0;j<MAX_COL_COUNT;j++) {
				// add
				if(!mCheckToDestroyed[i,j] && blownUpStatus[i,j].IsBlownUp()){
					mCheckToDestroyed[i,j] = true;
					
					if(blownUpStatus[i,j].mShape == BlownUpStatus.EffectShape.NONE){
						mTiles[i,j].Status.CountToDestroy--;
						if(mTiles[i,j].Status.CountToDestroy <= 0) {
							//Tile Destroyed :: give exp, money and special effects
							mTilesDestroyed[i,j] = true;
						}
					}
					else{
						int[] notDestroyedTilePath = SearchSameShape(blownUpStatus,i,j);
						if(notDestroyedTilePath[0] != -1 && notDestroyedTilePath[1] != -1){
							TileStatus nowStatus = mTiles[notDestroyedTilePath[0],notDestroyedTilePath[1]].Status;
							TileTypeManager.TileType madeSpecialType = TileTypeManager.TileType.NORMAL;
							mTilesDestroyed[notDestroyedTilePath[0], notDestroyedTilePath[1]] = false;
							mBlownTileCount++;
							
							if(blownUpStatus[i,j].mShape == BlownUpStatus.EffectShape.FOUR){
								madeSpecialType = TileTypeManager.TileType.HEAL;
							}
							else if(blownUpStatus[i,j].mShape == BlownUpStatus.EffectShape.L){
								madeSpecialType = TileTypeManager.TileType.CROSS;
							}
							else if(blownUpStatus[i,j].mShape == BlownUpStatus.EffectShape.FIVE){
								madeSpecialType = TileTypeManager.TileType.SPECIAL;
							}
							mTiles[notDestroyedTilePath[0],notDestroyedTilePath[1]].SetTile(new TileStatus(madeSpecialType, nowStatus.Color));
						}
					}
				}
			}
		}
		
		//Cross & heal & sepcial Effect.
		bool[,] isAlreadyBomb = new bool[MAX_ROW_COUNT, MAX_COL_COUNT];
		bool isStop = false;
		while(!isStop){
			isStop = true;
			for(i=0;i<MAX_ROW_COUNT;i++){
				for(j=0;j<MAX_COL_COUNT;j++){
					if(!isAlreadyBomb[i,j] && mTilesDestroyed[i,j]){
						isAlreadyBomb[i,j] = true;
						if(mTiles[i,j].Status.Type == TileTypeManager.TileType.HEAL){
							// Add Heal.
							UserManager.Instance.decreaseHP(-3);
							InGameUIManager.Instance.UpdateHP(UserManager.Instance.HP);
						}
						else if(mTiles[i,j].Status.Type == TileTypeManager.TileType.CROSS){
							int k;
							for(k=0;k<MAX_COL_COUNT;k++){
								if(mTiles[i,k].IsBlowable) mTilesDestroyed[i,k] = true;
							}
						}
						else if(mTiles[i,j].Status.Type == TileTypeManager.TileType.SPECIAL){
							// Special Effect.
							// Bubble Effect
							switch(mTiles[i,j].Status.Color){
								case TileTypeManager.TileColor.BLUE:
									MakeBubble();
									break;
							}
						}
						isStop = false;
					}
				}
			}
		}

		//Real delete
		int row;
		for(j=0;j<MAX_COL_COUNT;j++) {
			Queue<int> q = new Queue<int>();
			for(i=MAX_ROW_COUNT-1;i>=0;i--) {
				if(mTilesDestroyed[i, j]) {
					q.Enqueue(i);
					mBlownTileCount++;
				}else {
					if(q.Count > 0) {
						row = q.Dequeue();
						mTiles[row, j].IsBlowable = false;
						mTiles[row, j].SetPosition(mTiles[i,j].gameObject.transform.localPosition);
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
					Vector3 lastFellingTilePosition = mTiles[q.Count+1,j].gameObject.transform.localPosition;
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
		UserManager.Instance.setMP(mBaseMP + MP_INCREASING_CONSTANT * mBlownTileCount);
		InGameUIManager.Instance.UpdateMP(UserManager.Instance.MP);
		return true;
	}
	
	private void EnemyAction() {
		int i, j;
		
		mIsEnemyActionDone = true;
		if(!mIsBlownThisTurn) return;


		for(i=MAX_ROW_COUNT-1;i>=0;i--) {
			for(j=0;j<MAX_COL_COUNT;j++) {
				if(!TileTypeManager.Instance.IsEnemyType(mTiles[i,j].Status.Type)) continue;
				if(mTiles[i,j].Status.DecBubbleCount()) continue;
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
		mBlownTileCount = 0;
		mTurn ++;
		InGameUIManager.Instance.UpdateTurn(mTurn);
	}
}

		