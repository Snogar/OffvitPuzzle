using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TilesBlownUp{
	public bool Vertical;
	public bool Horizontal;
	public enum Kind{
		Trash,
		Five,
		L,
		Four
	};
	
	public Kind kind;
	public bool isBlownUp(){
		return Vertical || Horizontal;
	}
	public bool itsMe(Kind kk){
		if(kind == kk || kind == Kind.Trash){
			return true;
		}
		else return false;
	}
	public static void Construct(TilesBlownUp[,] tilesBlownUp){
		int i,j;
		int n,m;
		
		n = tilesBlownUp.GetLength(0);
		m = tilesBlownUp.GetLength(1);
		
		for(i=0;i<n;i++){
			for(j=0;j<m;j++){
				tilesBlownUp[i,j] = new TilesBlownUp();
			}
		}
	}
}

public class InGameLogicScript : MonoBehaviour {
	private const int MAX_ROW_COUNT = 8;
	private const int MAX_COL_COUNT = 8;
	private const int BLOW_MINIMUM_COUNT = 3;
	
	private TileScript[,] mTiles = new TileScript[MAX_ROW_COUNT, MAX_COL_COUNT];
	private bool mIsSwapEnable, mIsBlownThisTurn, mIsReSwapNeeded, mIsEnemyActionDone;
	private TileScript mClickedTile;
	private TileScript[] mLastSwappedTiles = new TileScript[2];
	private int mTurn;
	
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
		
		TilesBlownUp[,] tilesBlownUpTemp = new TilesBlownUp[MAX_ROW_COUNT, MAX_COL_COUNT];
		TilesBlownUp.Construct(tilesBlownUpTemp);
		
		while(CheckBlowUpTiles(tilesBlownUpTemp)) {
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
		destinationTile.IsBlowable = false;
		destinationTile.SetPosition(moveTile.GetTileVector());
		destinationTile.SetTile(moveTile.Status);
		StartCoroutine(InGameAnimationManager.Instance.MoveAnimation(destinationTile, destinationTile.GetTileVector()));
	}
	
	private void MoveTile(Vector3 startPosition, TileStatus tileStatus, TileScript destinationTile) {
		destinationTile.IsBlowable = false;
		destinationTile.SetPosition(startPosition);
		destinationTile.SetTile(tileStatus);
		StartCoroutine(InGameAnimationManager.Instance.MoveAnimation(destinationTile, destinationTile.GetTileVector()));
	}

	private bool CheckBlowUpTiles(TilesBlownUp [,] tilesBlownUp) {
		int i, j;
		bool isBlown = false;
		for(i=0;i<MAX_ROW_COUNT;i++) {
			for(j=0;j<MAX_COL_COUNT;j++) {
				if(!mTiles[i,j].IsBlowable) continue;
				/*
				//Right
				for(k=1;k<MAX_COL_COUNT-j;k++) {
					if(!mTiles[i,j+k].IsBlowable || mTiles[i,j].Status.Color != mTiles[i,j+k].Status.Color) break;
				}
				if(k >= BLOW_MINIMUM_COUNT) {
					tilesBlownUp[i, j] = true;
					isBlown = true;
					for(k=1;k<MAX_COL_COUNT-j;k++) {
						if(!mTiles[i,j+k].IsBlowable || mTiles[i,j].Status.Color != mTiles[i,j+k].Status.Color) break;
						tilesBlownUp[i, j+k] = true;
					}
				}
				
				//Down
				for(k=1;k<MAX_ROW_COUNT-i;k++) {
					if(!mTiles[i+k,j].IsBlowable || mTiles[i,j].Status.Color != mTiles[i+k,j].Status.Color) break;
				}
				if(k >= BLOW_MINIMUM_COUNT) {
					tilesBlownUp[i, j] = true;
					isBlown = true;
					for(k=1;k<MAX_ROW_COUNT-i;k++) {
						if(!mTiles[i+k,j].IsBlowable || mTiles[i,j].Status.Color != mTiles[i+k,j].Status.Color) break;
						tilesBlownUp[i+k, j] = true;
					}
				}*/
				if(i != 0 && i != MAX_ROW_COUNT-1){
					if(mTiles[i,j].IsBlowable && mTiles[i-1,j].IsBlowable && mTiles[i+1,j].IsBlowable){
						if(mTiles[i-1,j].Status.Color == mTiles[i,j].Status.Color &&
							mTiles[i+1,j].Status.Color == mTiles[i,j].Status.Color){
							tilesBlownUp[i,j].Vertical = true;
							tilesBlownUp[i,j].kind = TilesBlownUp.Kind.Trash;
							tilesBlownUp[i-1,j].Vertical = true;
							tilesBlownUp[i-1,j].kind = TilesBlownUp.Kind.Trash;
							tilesBlownUp[i+1,j].Vertical = true;
							tilesBlownUp[i+1,j].kind = TilesBlownUp.Kind.Trash;
							isBlown = true;
						}
					}
				}
				if(j != 0 && j != MAX_COL_COUNT-1){
					if(mTiles[i,j].IsBlowable && mTiles[i,j-1].IsBlowable && mTiles[i,j+1].IsBlowable){
						if(mTiles[i,j-1].Status.Color == mTiles[i,j].Status.Color &&
							mTiles[i,j+1].Status.Color == mTiles[i,j].Status.Color){
							tilesBlownUp[i,j].Horizontal = true;
							tilesBlownUp[i,j].kind = TilesBlownUp.Kind.Trash;
							tilesBlownUp[i,j-1].Horizontal = true;
							tilesBlownUp[i,j-1].kind = TilesBlownUp.Kind.Trash;
							tilesBlownUp[i,j+1].Horizontal = true;
							tilesBlownUp[i,j+1].kind = TilesBlownUp.Kind.Trash;
							isBlown = true;
						}
					}
				}
			}
		}
		
		//five 
		for(i=0;i<MAX_ROW_COUNT;i++) {
			for(j=0;j<MAX_COL_COUNT;j++) {
				// Vertical
				if(i >= 2 && i < MAX_ROW_COUNT-2){
					if(tilesBlownUp[i-2,j].isBlownUp() && 
						tilesBlownUp[i-1,j].isBlownUp()&& 
						tilesBlownUp[i,j].isBlownUp()&& 
						tilesBlownUp[i+1,j].isBlownUp()&& 
						tilesBlownUp[i+2,j].isBlownUp()){
						
						tilesBlownUp[i-2,j].kind = TilesBlownUp.Kind.Five;
						tilesBlownUp[i-1,j].kind = TilesBlownUp.Kind.Five;
						tilesBlownUp[i,j].kind = TilesBlownUp.Kind.Five;
						tilesBlownUp[i+1,j].kind = TilesBlownUp.Kind.Five;
						tilesBlownUp[i+2,j].kind = TilesBlownUp.Kind.Five;
					}
				}
				// Horizontal
				if(j >= 2 && j < MAX_COL_COUNT-2){
					if(tilesBlownUp[i,j-2].isBlownUp() && 
						tilesBlownUp[i,j-1].isBlownUp()&& 
						tilesBlownUp[i,j].isBlownUp()&& 
						tilesBlownUp[i,j+1].isBlownUp()&& 
						tilesBlownUp[i,j+2].isBlownUp()){
						
						tilesBlownUp[i,j-2].kind = TilesBlownUp.Kind.Five;
						tilesBlownUp[i,j-1].kind = TilesBlownUp.Kind.Five;
						tilesBlownUp[i,j].kind = TilesBlownUp.Kind.Five;
						tilesBlownUp[i,j+1].kind = TilesBlownUp.Kind.Five;
						tilesBlownUp[i,j+2].kind = TilesBlownUp.Kind.Five;
					}
				}
			}
		}
		
		//Giyeok
		for(i=0;i<MAX_ROW_COUNT;i++){
			for(j=0;j<MAX_COL_COUNT;j++){
				// Giyeok and right rotation
				//first Giyeok
				if(j >= 2 && i < MAX_ROW_COUNT-2){
					if(tilesBlownUp[i,j-2].isBlownUp() && 
						tilesBlownUp[i,j-1].isBlownUp()&& 
						tilesBlownUp[i,j].isBlownUp()&& 
						tilesBlownUp[i+1,j].isBlownUp()&& 
						tilesBlownUp[i+2,j].isBlownUp()){
						if(tilesBlownUp[i,j-2].itsMe (TilesBlownUp.Kind.L) &&
							tilesBlownUp[i,j-1].itsMe (TilesBlownUp.Kind.L) &&
							tilesBlownUp[i,j].itsMe (TilesBlownUp.Kind.L) &&
							tilesBlownUp[i+1,j].itsMe (TilesBlownUp.Kind.L)&&
							tilesBlownUp[i+2,j].itsMe (TilesBlownUp.Kind.L)){
						
							tilesBlownUp[i,j-2].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i,j-1].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i,j].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i+1,j].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i+2,j].kind = TilesBlownUp.Kind.L;
						}
					}
				}
				//second Giyeok
				if(j >= 2 && i >= 2){
					if(tilesBlownUp[i,j-2].isBlownUp() && 
						tilesBlownUp[i,j-1].isBlownUp()&& 
						tilesBlownUp[i,j].isBlownUp()&& 
						tilesBlownUp[i-1,j].isBlownUp()&& 
						tilesBlownUp[i-2,j].isBlownUp()){
						
						if(tilesBlownUp[i,j-2].itsMe (TilesBlownUp.Kind.L) &&
							tilesBlownUp[i,j-1].itsMe (TilesBlownUp.Kind.L) &&
							tilesBlownUp[i,j].itsMe (TilesBlownUp.Kind.L) &&
							tilesBlownUp[i-1,j].itsMe (TilesBlownUp.Kind.L)&&
							tilesBlownUp[i-2,j].itsMe (TilesBlownUp.Kind.L)){
						
							tilesBlownUp[i,j-2].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i,j-1].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i,j].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i-1,j].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i-2,j].kind = TilesBlownUp.Kind.L;
						}
					}
				}
				// third Giyeok
				if(j < MAX_COL_COUNT-2 && i >= 2){
					if(tilesBlownUp[i,j+2].isBlownUp() && 
						tilesBlownUp[i,j+1].isBlownUp()&& 
						tilesBlownUp[i,j].isBlownUp()&& 
						tilesBlownUp[i-1,j].isBlownUp()&& 
						tilesBlownUp[i-2,j].isBlownUp()){
						
						if(tilesBlownUp[i,j+2].itsMe (TilesBlownUp.Kind.L) &&
							tilesBlownUp[i,j+1].itsMe (TilesBlownUp.Kind.L) &&
							tilesBlownUp[i,j].itsMe (TilesBlownUp.Kind.L) &&
							tilesBlownUp[i-1,j].itsMe (TilesBlownUp.Kind.L)&&
							tilesBlownUp[i-2,j].itsMe (TilesBlownUp.Kind.L)){
						
							tilesBlownUp[i,j+2].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i,j+1].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i,j].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i-1,j].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i-2,j].kind = TilesBlownUp.Kind.L;
						}
					}
				}
				//fourth Giyeok
				if(j < MAX_COL_COUNT-2 && i < MAX_ROW_COUNT-2){
					if(tilesBlownUp[i,j+2].isBlownUp() && 
						tilesBlownUp[i,j+1].isBlownUp()&& 
						tilesBlownUp[i,j].isBlownUp()&& 
						tilesBlownUp[i+1,j].isBlownUp()&& 
						tilesBlownUp[i+2,j].isBlownUp()){
						
						if(tilesBlownUp[i,j+2].itsMe (TilesBlownUp.Kind.L) &&
							tilesBlownUp[i,j+1].itsMe (TilesBlownUp.Kind.L) &&
							tilesBlownUp[i,j].itsMe (TilesBlownUp.Kind.L) &&
							tilesBlownUp[i+1,j].itsMe (TilesBlownUp.Kind.L)&&
							tilesBlownUp[i+2,j].itsMe (TilesBlownUp.Kind.L)){
						
							tilesBlownUp[i,j+2].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i,j+1].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i,j].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i+1,j].kind = TilesBlownUp.Kind.L;
							tilesBlownUp[i+2,j].kind = TilesBlownUp.Kind.L;
						}
					}
				}
			}
		}
		//Four
		for(i=0;i<MAX_ROW_COUNT;i++) {
			for(j=0;j<MAX_COL_COUNT;j++) {
				// Vertical
				if(i >= 2 && i < MAX_ROW_COUNT-1){
					if( tilesBlownUp[i-2,j].isBlownUp() && 
						tilesBlownUp[i-1,j].isBlownUp()&& 
						tilesBlownUp[i,j].isBlownUp()&& 
						tilesBlownUp[i+1,j].isBlownUp()){
						if(tilesBlownUp[i-2,j].itsMe (TilesBlownUp.Kind.Four) &&
							tilesBlownUp[i-1,j].itsMe (TilesBlownUp.Kind.Four) &&
							tilesBlownUp[i,j].itsMe (TilesBlownUp.Kind.Four) &&
							tilesBlownUp[i+1,j].itsMe (TilesBlownUp.Kind.Four)){
							
							tilesBlownUp[i-2,j].kind = TilesBlownUp.Kind.Four;
							tilesBlownUp[i-1,j].kind = TilesBlownUp.Kind.Four;
							tilesBlownUp[i,j].kind = TilesBlownUp.Kind.Four;
							tilesBlownUp[i+1,j].kind = TilesBlownUp.Kind.Four;
						}
					}
				}
				// Horizontal
				if(j >= 2 && j < MAX_COL_COUNT-1){
					if( tilesBlownUp[i,j-2].isBlownUp() && 
						tilesBlownUp[i,j-1].isBlownUp()&& 
						tilesBlownUp[i,j].isBlownUp()&& 
						tilesBlownUp[i,j+1].isBlownUp()){
						if(tilesBlownUp[i,j-2].itsMe(TilesBlownUp.Kind.Four) &&
							tilesBlownUp[i,j-1].itsMe(TilesBlownUp.Kind.Four) &&
							tilesBlownUp[i,j].itsMe(TilesBlownUp.Kind.Four) &&
							tilesBlownUp[i,j+1].itsMe(TilesBlownUp.Kind.Four)){
							
							tilesBlownUp[i,j-2].kind = TilesBlownUp.Kind.Four;
							tilesBlownUp[i,j-1].kind = TilesBlownUp.Kind.Four;
							tilesBlownUp[i,j].kind = TilesBlownUp.Kind.Four;
							tilesBlownUp[i,j+1].kind = TilesBlownUp.Kind.Four;
						}
					}
				}
			}
		}
		return isBlown;
	}
	
	private bool BlowUpTiles() {
		int i, j;
		TilesBlownUp[,] tilesBlownUp = new TilesBlownUp[MAX_ROW_COUNT, MAX_COL_COUNT];
		TilesBlownUp.Construct(tilesBlownUp);
		
		bool[,] tilesDestroyed = new bool[MAX_ROW_COUNT, MAX_COL_COUNT];
		
		if(!CheckBlowUpTiles(tilesBlownUp)) return false;
		
		// delete 
		for(i=0;i<MAX_ROW_COUNT;i++) {
			for(j=0;j<MAX_COL_COUNT;j++) {
				// add
				if(tilesBlownUp[i,j].isBlownUp()) {
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
