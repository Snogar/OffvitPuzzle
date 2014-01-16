using UnityEngine;

public class TileStatus {
	private static int mMovingCount = 0;
	
	private int mCountToDestroy, mMovementSpeed, mAttackSpeed;
	private int mTurnLeftAttack;
	private TileTypeManager.TileType mType;
	private TileTypeManager.TileColor mColor;
	private int mMoveTime; // moving latest.
	private int mBubbleCount;
	private bool mDestroyed;
	private bool mFalling;
	private bool mIsEmpty;
	private int mFallingCount;
	private int mStep;

	public TileStatus() {
		mType = TypeGenerate();
		mColor = ColorGenerate();
		mCountToDestroy = TileTypeManager.Instance.GetCountToDestroy(mType);
		mMovementSpeed = TileTypeManager.Instance.GetMovementSpeed(mType);
		mAttackSpeed = TileTypeManager.Instance.GetAttackSpeed(mType);
		mTurnLeftAttack = 1;
		mBubbleCount = 0;
		mDestroyed = false;
		mFalling = false;
		mIsEmpty = false;
		mFallingCount = 0;
		mStep = 0;
		SetMoveTime();
	}
	
	public TileStatus(TileTypeManager.TileType type, TileTypeManager.TileColor color){
		mType = type;
		mColor = color;
		mCountToDestroy = TileTypeManager.Instance.GetCountToDestroy(mType);
		mMovementSpeed = TileTypeManager.Instance.GetMovementSpeed(mType);
		mAttackSpeed = TileTypeManager.Instance.GetAttackSpeed(mType);
		mTurnLeftAttack = 1;
		mBubbleCount = 0;
		mDestroyed = false;
		mFalling = false;
		mIsEmpty = false;
		mFallingCount = 0;
		mStep = 0;
		SetMoveTime();
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

	public bool Destroyed {
		get { return mDestroyed; }
		set { mDestroyed = value; }
	}
	public bool Falling {
		get { return mFalling; }
		set { mFalling = value;
			mFallingCount = 0;}
	}
	public int FallingCount{
		get { return mFallingCount; }
		set { mFallingCount=value;}
	}
	public bool IsEmpty{
		get { return mIsEmpty; }
		set { mIsEmpty=value;}
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
		else if(randomSeed <= 30) return TileTypeManager.TileType.SPECIAL;
		else if(randomSeed <= 50) return TileTypeManager.TileType.CROSS;
		return TileTypeManager.TileType.NORMAL;
	}
	private TileTypeManager.TileColor ColorGenerate() {
		return (TileTypeManager.TileColor)Random.Range(0, (int)(TileTypeManager.TileColor.MAX_COUNT));
	}
	public void AttackTurnReset() {
		mTurnLeftAttack = mAttackSpeed;
	}
	public void IncFallingCount(){
		mFallingCount ++;
	}
	public void SetBubbleCount(){
		mBubbleCount = 2;
	}
	public bool DecBubbleCount(){
		if(mBubbleCount > 0){
			mBubbleCount --;
			return true;
		}
		return false;
	}
	public void SetMoveTime() {
		mMoveTime = mMovingCount++;
	}
}
