using System;
using System.Collections.Generic;


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
	
	public BlownUpStatus()
	{
		this.Vertical = false;
		this.Horizontal = false;
		this.Shape = EffectShape.NONE;
	}
	
	public bool IsBlownUp(){
		return Vertical || Horizontal;
	}
	
	public bool IsSameEffectShape(EffectShape centerShape){
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
	
	public static void SetShape(BlownUpStatus[,] blownUpStatus, int pivotRow, int pivotCol, int[][] IndexList, EffectShape shape)
	{
		foreach(int[] index in IndexList){
			int currentRow = pivotRow + index[0];
			int currentCol = pivotCol + index[1];
			
			if(!blownUpStatus[currentRow, currentCol].IsSameEffectShape(shape)) return;
		}
		
		foreach(int[] index in IndexList){
			int currentRow = pivotRow + index[0];
			int currentCol = pivotCol + index[1];
			
			blownUpStatus[currentRow, currentCol].Shape = shape;
		}
	}
}
