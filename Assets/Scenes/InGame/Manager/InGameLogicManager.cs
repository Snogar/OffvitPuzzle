using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class InGameLogicManager : MonoBehaviour {
	public const int MAX_ROW_COUNT = 8;
	public const int MAX_COL_COUNT = 8;
	private const int BLOW_MINIMUM_COUNT = 3;
	private const int MP_INCREASING_CONSTANT = 3;
	private const int MAX_MP = 400;
	
	private TileScript[,] mTiles = new TileScript[MAX_ROW_COUNT, MAX_COL_COUNT];
	private bool mIsSwapEnable, mIsBlownThisTurn, mIsReSwapNeeded, mIsEnemyActionDone;
	private TileScript mClickedTile;
	private TileScript[] mLastSwappedTiles = new TileScript[2];
	private int mTurn;
	private int mBlownTileCount;
	private int mBaseMP;
	
	private bool[,] mCheckToDestroyed;
	public TileScript[,] Tiles{
		get { return mTiles; }
	}
	private static int[,] LOC_ALL = new int[225,2] {
		{-7, -7}, {-7, -6}, {-7, -5}, {-7, -4}, {-7, -3}, {-7, -2}, {-7, -1}, {-7, 0}, {-7, 1}, {-7, 2}, {-7, 3}, {-7, 4}, {-7, 5}, {-7, 6}, {-7, 7},
		{-6, -7}, {-6, -6}, {-6, -5}, {-6, -4}, {-6, -3}, {-6, -2}, {-6, -1}, {-6, 0}, {-6, 1}, {-6, 2}, {-6, 3}, {-6, 4}, {-6, 5}, {-6, 6}, {-6, 7},
		{-5, -7}, {-5, -6}, {-5, -5}, {-5, -4}, {-5, -3}, {-5, -2}, {-5, -1}, {-5, 0}, {-5, 1}, {-5, 2}, {-5, 3}, {-5, 4}, {-5, 5}, {-5, 6}, {-5, 7},
		{-4, -7}, {-4, -6}, {-4, -5}, {-4, -4}, {-4, -3}, {-4, -2}, {-4, -1}, {-4, 0}, {-4, 1}, {-4, 2}, {-4, 3}, {-4, 4}, {-4, 5}, {-4, 6}, {-4, 7},
		{-3, -7}, {-3, -6}, {-3, -5}, {-3, -4}, {-3, -3}, {-3, -2}, {-3, -1}, {-3, 0}, {-3, 1}, {-3, 2}, {-3, 3}, {-3, 4}, {-3, 5}, {-3, 6}, {-3, 7},
		{-2, -7}, {-2, -6}, {-2, -5}, {-2, -4}, {-2, -3}, {-2, -2}, {-2, -1}, {-2, 0}, {-2, 1}, {-2, 2}, {-2, 3}, {-2, 4}, {-2, 5}, {-2, 6}, {-2, 7},
		{-1, -7}, {-1, -6}, {-1, -5}, {-1, -4}, {-1, -3}, {-1, -2}, {-1, -1}, {-1, 0}, {-1, 1}, {-1, 2}, {-1, 3}, {-1, 4}, {-1, 5}, {-1, 6}, {-1, 7},
		{ 0, -7}, { 0, -6}, { 0, -5}, { 0, -4}, { 0, -3}, { 0, -2}, { 0, -1}, { 0, 0}, { 0, 1}, { 0, 2}, { 0, 3}, { 0, 4}, { 0, 5}, { 0, 6}, { 0, 7},
		{ 1, -7}, { 1, -6}, { 1, -5}, { 1, -4}, { 1, -3}, { 1, -2}, { 1, -1}, { 1, 0}, { 1, 1}, { 1, 2}, { 1, 3}, { 1, 4}, { 1, 5}, { 1, 6}, { 1, 7},
		{ 2, -7}, { 2, -6}, { 2, -5}, { 2, -4}, { 2, -3}, { 2, -2}, { 2, -1}, { 2, 0}, { 2, 1}, { 2, 2}, { 2, 3}, { 2, 4}, { 2, 5}, { 2, 6}, { 2, 7},
		{ 3, -7}, { 3, -6}, { 3, -5}, { 3, -4}, { 3, -3}, { 3, -2}, { 3, -1}, { 3, 0}, { 3, 1}, { 3, 2}, { 3, 3}, { 3, 4}, { 3, 5}, { 3, 6}, { 3, 7},
		{ 4, -7}, { 4, -6}, { 4, -5}, { 4, -4}, { 4, -3}, { 4, -2}, { 4, -1}, { 4, 0}, { 4, 1}, { 4, 2}, { 4, 3}, { 4, 4}, { 4, 5}, { 4, 6}, { 4, 7},
		{ 5, -7}, { 5, -6}, { 5, -5}, { 5, -4}, { 5, -3}, { 5, -2}, { 5, -1}, { 5, 0}, { 5, 1}, { 5, 2}, { 5, 3}, { 5, 4}, { 5, 5}, { 5, 6}, { 5, 7},
		{ 6, -7}, { 6, -6}, { 6, -5}, { 6, -4}, { 6, -3}, { 6, -2}, { 6, -1}, { 6, 0}, { 6, 1}, { 6, 2}, { 6, 3}, { 6, 4}, { 6, 5}, { 6, 6}, { 6, 7},
		{ 7, -7}, { 7, -6}, { 7, -5}, { 7, -4}, { 7, -3}, { 7, -2}, { 7, -1}, { 7, 0}, { 7, 1}, { 7, 2}, { 7, 3}, { 7, 4}, { 7, 5}, { 7, 6}, { 7, 7}
	};
	private static int[,] LOC_MANHATTAN_DISTANCE_2 = new int[13,2] {{-2, 0}, {-1, -1}, {-1, 0}, {-1, 1}, {0, -2}, {0, -1}, {0, 0}, {0, 1}, {0, 2}, {1, -1}, {1, 0}, {1, 1}, {2, 0}};

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
	private void check(){
		int i,j;
		for(i=0;i<MAX_ROW_COUNT;i++){
			for(j=0;j<MAX_COL_COUNT;j++){
				if(mTiles[i,j].transform.localScale.x.Equals(0.0f)){
					if(!mTiles[i,j].Status.IsEmpty){
						Debug.Log ("Is Blowable : " + mTiles[i,j].IsBlowable);
					}
				}
			}
		}
	}
	private void Update () {
		check();
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
			mTiles[now[0],now[1]].Status.Destroyed = true;
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
	private int CalculateMP(int baseMP, int blownTileCount){
		int newMP = baseMP + MP_INCREASING_CONSTANT * mBlownTileCount;
		if(newMP >= MAX_MP) return MAX_MP;
		return newMP;
	}

	private int MAP<T>(T[,] array, int row, int col, int[,] locations, Predicate<T> filter, Action<T> action){
		int rowLength = array.GetLength(0);
		int colLength = array.GetLength(1);
		int count = 0;
		for(int i=0;i<locations.GetLength(0);i++){
			int curRow = row + locations[i, 0];
			int curCol = col + locations[i, 1];
			if(!(curRow >= 0 && curRow < rowLength
			     && curCol >= 0 && curCol < colLength
			     && filter(array[curRow, curCol]))) continue;
			count++;
			action(array[curRow, curCol]);
		}
		return count;
	}
	private void DamageTile(TileScript tile, int damage) {
		if(!tile.IsBlowable || tile.Status.Destroyed) return;
		tile.Status.CountToDestroy-= damage;
		if(tile.Status.CountToDestroy <= 0) {
			//Tile Destroyed :: give exp, money and special effects
			mBlownTileCount++;
			tile.Status.Destroyed = true;
		}
	}
	private bool BlowUpTiles() {
		int i, j;
		BlownUpStatus[,] blownUpStatus = new BlownUpStatus[MAX_ROW_COUNT, MAX_COL_COUNT];
		BlownUpStatus.Construct(blownUpStatus);
		
		if(!CheckBlowUpTiles(blownUpStatus)) return false;
		mCheckToDestroyed = new bool[MAX_ROW_COUNT,MAX_COL_COUNT];
		// delete
		for(i=0;i<MAX_ROW_COUNT;i++) {
			for(j=0;j<MAX_COL_COUNT;j++) {
				// add
				if(!mCheckToDestroyed[i,j] && blownUpStatus[i,j].IsBlownUp()){
					mCheckToDestroyed[i,j] = true;
					
					if(blownUpStatus[i,j].mShape == BlownUpStatus.EffectShape.NONE){
						DamageTile(mTiles[i,j], 1);
					}
					else{
						int[] notDestroyedTilePath = SearchSameShape(blownUpStatus,i,j);
						if(notDestroyedTilePath[0] != -1 && notDestroyedTilePath[1] != -1){
							DamageTile(mTiles[i,j], 1);
							//Revive Special Tile
							mTiles[notDestroyedTilePath[0], notDestroyedTilePath[1]].Status.Destroyed = false;
							TileStatus nowStatus = mTiles[notDestroyedTilePath[0],notDestroyedTilePath[1]].Status;
							TileTypeManager.TileType madeSpecialType = TileTypeManager.TileType.NORMAL;
														
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
					if(!isAlreadyBomb[i,j] && mTiles[i,j].Status.Destroyed){
						isAlreadyBomb[i,j] = true;
						if(mTiles[i,j].Status.Type == TileTypeManager.TileType.HEAL){
							// Add Heal.
							UserManager.Instance.decreaseHP(-3);
							InGameUIManager.Instance.UpdateHP(UserManager.Instance.HP);
						}
						else if(mTiles[i,j].Status.Type == TileTypeManager.TileType.CROSS){
							int k;
							for(k=0;k<MAX_COL_COUNT;k++){
								DamageTile(mTiles[i,k], 1);
							}
						}
						else if(mTiles[i,j].Status.Type == TileTypeManager.TileType.SPECIAL){
							// Special Effect.
							// Bubble Effect
							switch(mTiles[i,j].Status.Color){
								case TileTypeManager.TileColor.BLUE:
									MAP<TileScript>(mTiles, i, j, LOC_ALL, 
								    	            delegate(TileScript tile) { return TileTypeManager.Instance.IsEnemyType(tile.Status.Type); },
													delegate(TileScript tile) { tile.Status.SetBubbleCount(); });
									break;
								case TileTypeManager.TileColor.WHITE:
									MAP<TileScript>(mTiles, i, j, LOC_ALL,
									                delegate(TileScript tile) { return tile.Status.Color == TileTypeManager.TileColor.WHITE; },
													delegate(TileScript tile) { DamageTile(tile, 1); });
									break;
								case TileTypeManager.TileColor.PINK:
									MAP<TileScript>(mTiles, i, j, LOC_MANHATTAN_DISTANCE_2,
								                	delegate(TileScript tile) { return true; },
													delegate(TileScript tile) { DamageTile(tile, 1); });
									break;
							}
						}
						isStop = false;
					}
				}
			}
		}

		//Real delete
//		int row;
//		for(j=0;j<MAX_COL_COUNT;j++) {
//			Queue<int> q = new Queue<int>();
//			for(i=MAX_ROW_COUNT-1;i>=0;i--) {
//				if(mTiles[i, j].Status.Destroyed) {
//					//mTiles[i, j].Status.Destroyed = true;
//					q.Enqueue(i);
//				}else {
//					if(q.Count > 0) {
//						row = q.Dequeue();
//						mTiles[row, j].IsBlowable = false;
//						mTiles[row, j].SetPosition(mTiles[i,j].gameObject.transform.localPosition);
//						mTiles[row, j].SetTile(mTiles[i,j].Status);
//						StartCoroutine(InGameAnimationManager.Instance.TileMoveToOriginalPositionStart(mTiles[row, j]));
//						q.Enqueue(i);
//					}
//				}
//			}
//
//			while(q.Count > 0) {
//				row = q.Dequeue();
//				mTiles[row, j].IsBlowable = false;
//				Vector3 topTilePosition = TileScript.GetTileVectorWithRowCol(0, j);
//				if(q.Count + 1 < MAX_ROW_COUNT) {
//					Vector3 lastFellingTilePosition = mTiles[q.Count+1,j].gameObject.transform.localPosition;
//					if(lastFellingTilePosition.y <= topTilePosition.y) 
//						mTiles[row, j].SetPosition(new Vector3(topTilePosition.x, topTilePosition.y+TileScript.tileSize, 0));
//					else 
//						mTiles[row, j].SetPosition(new Vector3(lastFellingTilePosition.x, lastFellingTilePosition.y+TileScript.tileSize, 0));
//				}else {
//					mTiles[row, j].SetPosition(new Vector3(topTilePosition.x, topTilePosition.y+TileScript.tileSize, 0));
//				}
//				mTiles[row, j].SetTile(new TileStatus());
//				StartCoroutine(InGameAnimationManager.Instance.TileMoveToOriginalPositionStart(mTiles[row, j]));
//			}
//		}	
		for(j=0;j<MAX_COL_COUNT;j++){
			for(i=MAX_ROW_COUNT-1;i>=0;i--){
				if(mTiles[i,j].Status.Destroyed){
					mTiles[i,j].Status.Destroyed = false;
					mTiles[i,j].IsBlowable = false;
					StartCoroutine(InGameAnimationManager.Instance.TileDestroy(mTiles[i, j]));
				}
			}
		}
		UserManager.Instance.setMP(CalculateMP(mBaseMP, mBlownTileCount));
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

	public void TileDestroyEnd(TileScript tile){
		int x = tile.Col,y = tile.Row;
		int i;
		tile.Status.IsEmpty = true;
		for(i=y;i>=0;i--){
			if(!mTiles[i,x].Status.Falling){
				mTiles[i,x].Status.Falling = true;
				mTiles[i,x].IsBlowable = false;
				TileFallingStart(mTiles[i,x]);
			}
		}
	}
	public void TileFallingStart(TileScript tile){
		int x = tile.Col,y = tile.Row;
		if(!tile.Status.Falling){
			Debug.Log ("FallingStart  "+y+"  ,  "+x);
		}
		if(y + 1 < MAX_ROW_COUNT){
			if(mTiles[y+1,x].Status.Falling || mTiles[y+1,x].Status.IsEmpty){
				StartCoroutine(InGameAnimationManager.Instance.TileFalling(mTiles[y,x]));
				if(y == 0){
					MakingNewTile(x);
				}
			}
			else if(y == 0 && mTiles[y,x].Status.IsEmpty){
				MakingNewTile(x);
			}
			else{
				if(!mTiles[y,x].Status.IsEmpty){
					mTiles[y,x].IsBlowable = true;
					mTiles[y,x].Status.Falling = false;
				}
			}
		}
		else{
			if(!mTiles[y,x].Status.IsEmpty){
				mTiles[y,x].IsBlowable = true;
				mTiles[y,x].Status.Falling = false;
			}
		}
	}
	public void MakingNewTile(int x){
		GameObject tileObject = Resources.Load("InGame/Tile", typeof(GameObject)) as GameObject;
		GameObject tileObjectClone = (GameObject)Instantiate(tileObject);
		tileObjectClone.name = "Tile(" + -1 + "," + x + ")";
		TileScript newTileScript = tileObjectClone.GetComponent<TileScript>();
		newTileScript.Init(-1, x, new TileStatus());
		newTileScript.IsBlowable = false;
		newTileScript.Status.Falling = true;
		newTileScript.Status.FallingCount = 0;
		StartCoroutine(InGameAnimationManager.Instance.TileFalling(newTileScript));
	}
	public void TileFallingEnd(TileScript tile){
		int x = tile.Col, y = tile.Row;
		mTiles[y+1,x].Init(y+1,x,tile.Status);
		if(y == -1){
			Destroy (tile.gameObject);
		}
		if(mTiles[y+1,x].Status.IsEmpty){
			mTiles[y+1,x].transform.localScale = new Vector3(0.0f,0.0f,0.0f);
		}
		mTiles[y+1,x].IsBlowable = false;
		TileFallingStart(mTiles[y+1,x]);
	}
}	