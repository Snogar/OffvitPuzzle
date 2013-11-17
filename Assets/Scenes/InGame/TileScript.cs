using UnityEngine;

public class TileScript : MonoBehaviour { 
	private static tk2dSpriteCollectionData collection;
	public const int tileSize = 80;
	
	private int mRow, mCol;
	private tk2dSprite mSprite;
	private bool mIsBlowable;
	
	public TileStatus mStatus;
	
	//
	private static tk2dSpriteCollectionData Collection {
		get {
			if(collection == null) {
				collection = Resources.Load("LoLTile/LoLTileSpriteCollection", typeof(tk2dSpriteCollectionData)) as tk2dSpriteCollectionData;
			}
			return collection;	
		}
	}
	
	public static Vector3 GetTileVectorWithRowCol(int row, int col) {
		return new Vector3(col*tileSize + tileSize/2, 960 - tileSize/2 - row*tileSize, 0);
	}
	
	public static bool HasSameColor(TileScript[,] mTiles, int pivotRow, int pivotCol, int[][] IndexList)
	{
		int rowMax = mTiles.GetLength(0);
		int colMax = mTiles.GetLength(1);
		
		TileTypeManager.TileColor pivotColor = mTiles[pivotRow, pivotCol].Status.Color;
		
		foreach(int[] index in IndexList){
			int currentRow = pivotRow + index[0];
			int currentCol = pivotCol + index[1];
			
			if(currentRow < 0 || currentRow >= rowMax || currentCol < 0 || currentCol >= colMax) return false;
			
			if(mTiles[currentRow, currentCol].Status.Color != pivotColor) return false;
		}
		return true;
	}
	
	public Vector3 GetTileVector() {
		return GetTileVectorWithRowCol(mRow, mCol);
	}
	
	public int Row {
		get { return mRow; }
	}
	
	public int Col {
		get { return mCol; }
	}
	
	public TileStatus Status {
		get { return mStatus; }
	}
	
	public tk2dSprite Sprite {
		get { return mSprite; }
	}
	
	public bool IsBlowable {
		get { return mIsBlowable; }
		set { mIsBlowable = value; }
	}
	
	public void SetPosition(Vector3 position) {
		gameObject.transform.position = new Vector3(position.x, position.y, gameObject.transform.position.z);
	}
	//
	
	public void Init (int row, int col, TileStatus status) {
		mRow = row;
		mCol = col;
		mIsBlowable = true;
		mSprite = gameObject.AddComponent<tk2dSprite>();
		gameObject.transform.parent = InGameLogicScript.Instance.transform;
		gameObject.transform.position = GetTileVector();
		
		SetTile(status);
		mSprite.MakePixelPerfect();
	}

	public void SetTile (TileStatus status) {
		mStatus = status;
		mSprite.SetSprite (Collection, TileTypeManager.Instance.SpriteName(status.Type, status.Color));
		if(TileTypeManager.Instance.IsEnemyType(status.Type)) {
			gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -1);
		}
		else {
			gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0);
		}
		
		gameObject.transform.localScale = TileTypeManager.Instance.GetTileScale(status.Type);
	}
	
	private void Start () {		
	}

	private void Update () {
	}
}

public class TileStatus {
	private int mCountToDestroy, mMovementSpeed, mAttackSpeed;
	private int mTurnLeftAttack;
	private TileTypeManager.TileType mType;
	private TileTypeManager.TileColor mColor;
	
	private int mMoveTime; // moving latest.
	
	public TileStatus() {
		mType = TypeGenerate();
		mColor = ColorGenerate();
		mCountToDestroy = TileTypeManager.Instance.GetCountToDestroy(mType);
		mMovementSpeed = TileTypeManager.Instance.GetMovementSpeed(mType);
		mAttackSpeed = TileTypeManager.Instance.GetAttackSpeed(mType);
		mMoveTime = 0;
		
		mTurnLeftAttack = 1;
	}
	public int MoveTime {
		get { return mMoveTime; }
		set { mMoveTime = value; }
	}
	public TileTypeManager.TileType Type {
		get { return mType; }
		set { mType = value; }
	}
	
	public TileTypeManager.TileColor Color {
		get { return mColor; }
		set { mColor = value; }
	}
	
	public int CountToDestroy {
		get { return mCountToDestroy; }
		set { mCountToDestroy = value; }
	}
	
	public int MovementSpeed {
		get { return mMovementSpeed; }
		set { mMovementSpeed = value; }
	}
	
	public int AttackSpeed {
		get { return mAttackSpeed; }
		//set { mAttackSpeed = value; }
	}
	
	public int TurnLeftAttack {
		get { return mTurnLeftAttack; }
		set { mTurnLeftAttack = value; }
	}
	
	private TileTypeManager.TileType TypeGenerate() {
		//TODO : Probability Modification
		int randomSeed = Random.Range(0, 100);
		if(randomSeed <= 1) return TileTypeManager.TileType.ENEMY_WARRIOR;
		else if(randomSeed <= 3) return TileTypeManager.TileType.ENEMY_ARCHER;
		else if(randomSeed <= 5) return TileTypeManager.TileType.ENEMY_GIANT;
		else if(randomSeed <= 7) return TileTypeManager.TileType.ENEMY_ASSASSIN;
		else if(randomSeed <= 9) return TileTypeManager.TileType.ENEMY_WIZARD;
		return TileTypeManager.TileType.NORMAL;
	}
	private TileTypeManager.TileColor ColorGenerate() {
		return (TileTypeManager.TileColor)Random.Range(0, (int)(TileTypeManager.TileColor.MAX_COUNT));
	}
	
	public void AttackTurnReset() {
		mTurnLeftAttack = mAttackSpeed;
	}
}