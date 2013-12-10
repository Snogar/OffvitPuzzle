using System;
using System.Collections.Generic;


public class BlownUpStatus
{
	public bool mVertical;
	public bool mHorizontal;
	public enum EffectShape{
		NONE,
		FIVE,
		L,
		FOUR
	};
	public EffectShape mShape;
	
	public BlownUpStatus()
	{
		this.mVertical = false;
		this.mHorizontal = false;
		this.mShape = EffectShape.NONE;
	}
	
	public bool IsBlownUp(){
		return mVertical || mHorizontal;
	}
	
	public bool IsSameEffectShape(EffectShape centerShape){
		if(this.mShape == centerShape || this.mShape == EffectShape.NONE){
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
	
	public static void SetShape(BlownUpStatus[,] blownUpStatus, int pivotRow, int pivotCol, int[,] indexList, EffectShape shape)
	{
		int i;
		for(i=0;i<indexList.GetLength(0);i++) {
			int currentRow = pivotRow + indexList[i, 0];
			int currentCol = pivotCol + indexList[i, 1];
			
			if(!blownUpStatus[currentRow, currentCol].IsSameEffectShape(shape)) return;
		}
		
		for(i=0;i<indexList.GetLength(0);i++) {
			int currentRow = pivotRow + indexList[i, 0];
			int currentCol = pivotCol + indexList[i, 1];
			
			blownUpStatus[currentRow, currentCol].mShape = shape;
		}
	}
}
