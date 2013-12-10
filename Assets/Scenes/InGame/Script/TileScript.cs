using UnityEngine;

public class TileScript : MonoBehaviour { 
	private static UIAtlas tileAtlas;
	private static GameObject tileObject;
	public const int tileSize = 80;
	
	private int mRow, mCol;
	private UISprite mSprite;
	private bool mIsBlowable;
	
	public TileStatus mStatus;
	
	public static Vector3 GetTileVectorWithRowCol(int row, int col) {
		return new Vector3(col*tileSize - tileSize*3.5f, -row*tileSize + tileSize*5, 0);
	}
	
	public static bool HasSameColor(TileScript[,] mTiles, int pivotRow, int pivotCol, int[,] indexList)
	{
		int rowMax = mTiles.GetLength(0);
		int colMax = mTiles.GetLength(1);
		
		TileTypeManager.TileColor pivotColor = mTiles[pivotRow, pivotCol].Status.Color;
		
		for(int i=0; i<indexList.GetLength(0); i++) {
			int currentRow = pivotRow + indexList[i, 0];
			int currentCol = pivotCol + indexList[i, 1];
			
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
	
	public UISprite Sprite {
		get { return mSprite; }
	}
	
	public bool IsBlowable {
		get { return mIsBlowable; }
		set { mIsBlowable = value; }
	}
	
	public void SetPosition(Vector3 position) {
		gameObject.transform.localPosition = new Vector3(position.x, position.y, gameObject.transform.localPosition.z);
	}
	//
	
	public void Init (int row, int col, TileStatus status) {
		mRow = row;
		mCol = col;
		mIsBlowable = true;
		mSprite = gameObject.transform.Find("TileSprite").GetComponent<UISprite>();
		gameObject.transform.parent = InGameLogicManager.Instance.transform;
		gameObject.transform.localPosition = GetTileVector();
		
		SetTile(status);
		mSprite.MakePixelPerfect();
	}
	
	public void SetTile (TileStatus status) {
		mStatus = status;
		mSprite.spriteName = TileTypeManager.Instance.SpriteName(status.Type, status.Color);
		if(TileTypeManager.Instance.IsEnemyType(status.Type)) {
			//gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, -1);
			mSprite.depth = 1;
		}
		else {
			mSprite.depth = 0;
			//gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, 0);
		}
		
		gameObject.transform.localScale = TileTypeManager.Instance.GetTileScale(status.Type);
	}
	
	private void Start () {		
	}

	private void Update () {
	}
}
