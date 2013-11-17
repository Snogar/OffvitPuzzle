using System;


public class BlownUpStatus
{
	public bool Vertical;
	public bool Horizontal;
	public enum EffectShape{
		NONE,
		FIVE,
		L,
		FOUR
	};
	
	public EffectShape Shape;
	public bool isBlownUp(){
		return Vertical || Horizontal;
	}
	public bool isSameEffectShape(EffectShape centerShape){
		if(this.Shape == centerShape || this.Shape == EffectShape.NONE){
			return true;
		}
		else return false;
	}
	public static void Construct(BlownUpStatus[,] blownUpStatus){
		int row = blownUpStatus.GetLength(0);
		int col = blownUpStatus.GetLength(1);
		
		for(int i=0;i<row;i++){
			for(int j=0;j<col;j++){
				blownUpStatus[i,j] = new BlownUpStatus();
			}
		}
	}
}
