
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ConnectFour
{

	public class GameController : MonoBehaviour 
	{

		BrainConnect player1;
		BrainConnect player2;
		enum Piece
		{
			Empty = 0,
			Red = 1,
			Yellow = 2
		}

		[Range(3, 8)]
		public int numRows = 6;
		[Range(3, 8)]
		public int numColumns = 7;

		[Tooltip("How many pieces have to be connected to win.")]
		public int numPiecesToWin = 4;

		[Tooltip("Allow diagonally connected Pieces?")]
		public bool allowDiagonally = true;
		
		public float dropTime = 4f;
		public float dropOutTime = 0.001f;

		[Tooltip("How often to errorerrorerror")]
		public int howOftenDrop = 3;
		int howLongSinceDropOut = 0;

		// Gameobjects 
		public GameObject pieceRed;
		public GameObject pieceYellow;
		public GameObject pieceField;

		public GameObject winningText;
		public string redPlayerWinText = "Red Won!";
		public string yellowPlayerWinText = "Yellow Won!";
		public string drawText = "Draw!";

		public GameObject btnPlayAgain;
		bool btnPlayAgainTouching = false;
		Color btnPlayAgainOrigColor;
		Color btnPlayAgainHoverColor = new Color(255, 143,4);

		GameObject gameObjectField;

		// temporary gameobject, holds the piece at mouse position until the mouse has clicked
		GameObject gameObjectTurn;

		/// <summary>
		/// The Game field.
		/// 0 = Empty
		/// 1 = Red
		/// 2 = Yellow
		/// </summary>
		int[,] field;

		bool whichPlayersTurn = true;
		// 0 = red
		// 1 = yellow

		bool isLoading = true;
		bool isDropping = false; 
		bool mouseButtonPressed = false;

		bool gameOver = false;
		bool isCheckingForWinner = false;

		// Use this for initialization
		void Start () 
		{

			player1 = GameObject.FindGameObjectWithTag("Player1").GetComponent<BrainConnect>();
			int max = Mathf.Max (numRows, numColumns);

			if(numPiecesToWin > max)
				numPiecesToWin = max;

			CreateField ();

			whichPlayersTurn = System.Convert.ToBoolean(Random.Range (0, 1));
			// 0 = red, 1 = yellow

			btnPlayAgainOrigColor = btnPlayAgain.GetComponent<Renderer>().material.color;
		}

		/// <summary>
		/// Creates the field.
		/// </summary>
		void CreateField()
		{
			winningText.SetActive(false);
			btnPlayAgain.SetActive(false);

			isLoading = true;

			gameObjectField = GameObject.Find ("Field");
			if(gameObjectField != null)
			{
				DestroyImmediate(gameObjectField);
			}
			gameObjectField = new GameObject("Field");

			// create an empty field and instantiate the cells
			field = new int[numColumns, numRows];
			for(int x = 0; x < numColumns; x++)
			{
				for(int y = 0; y < numRows; y++)
				{
					field[x, y] = (int)Piece.Empty;
					GameObject g = Instantiate(pieceField, new Vector3(x, y * -1, -1), Quaternion.identity) as GameObject;
					g.transform.parent = gameObjectField.transform;
				}
			}

			isLoading = false;
			gameOver = false;

			// center camera
			Camera.main.transform.position = new Vector3(
				(numColumns-1) / 2.0f, -((numRows-1) / 2.0f), Camera.main.transform.position.z);

			winningText.transform.position = new Vector3(
				(numColumns-1) / 2.0f, -((numRows-1) / 2.0f) + 1, winningText.transform.position.z);

			btnPlayAgain.transform.position = new Vector3(
				(numColumns-1) / 2.0f, -((numRows-1) / 2.0f) - 1, btnPlayAgain.transform.position.z);
		}

		/// <summary>
		/// Spawns a piece at mouse position above the left column
		/// </summary>
		/// <returns>The piece.</returns>
		GameObject SpawnPiece()
		{
			Vector3 spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
					
			GameObject g = Instantiate(
					whichPlayersTurn ? pieceYellow : pieceRed, // player 1 = yellow, player 0 = red
					new Vector3(
					Mathf.Clamp(spawnPos.x, 0, numColumns-1), 
					gameObjectField.transform.position.y + 1, 0), // spawn it above the left column
					Quaternion.identity) as GameObject;

			return g;
		}

		void UpdatePlayAgainButton()
		{
			RaycastHit hit;
			//ray shooting out of the camera from where the mouse is
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			
			if (Physics.Raycast(ray, out hit) && hit.collider.name == btnPlayAgain.name)
			{
				btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainHoverColor;
				//check if the left mouse has been pressed down this frame
				if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && btnPlayAgainTouching == false)
				{
					btnPlayAgainTouching = true;
					
					//CreateField();
					Application.LoadLevel(0);
				}
			}
			else
			{
				btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainOrigColor;
			}
			
			if(Input.touchCount == 0)
			{
				btnPlayAgainTouching = false;
			}
		}

		// Update is called once per frame
		void Update () 
		{
			if(isLoading)
				return;

			if(isCheckingForWinner)
				return;

			if(gameOver)
			{
				winningText.SetActive(true);
				btnPlayAgain.SetActive(true);

				UpdatePlayAgainButton();

				return;
			}

			if (howLongSinceDropOut == howOftenDrop)
			{
				Debug.Log("drop");
				howLongSinceDropOut = 0;
				// drop pieces
				int whichColumn = Random.Range(0,numColumns);


				// set all the pieces in that column to be empty
				for(int y = 0; y < numRows; y++)
				{
					field[whichColumn, y] = (int)Piece.Empty;
					//GameObject g = Instantiate(pieceField, new Vector3(whichColumn, y * -1, -1), Quaternion.identity) as GameObject;
					//g.transform.parent = gameObjectField.transform;
				}

				// delete existing game pieces in that column
				GameObject[] gamePieces;
			    gamePieces = GameObject.FindGameObjectsWithTag("gamePiece");

			    foreach (GameObject gamePiece in gamePieces)
		        {
		        	// check if the gamepiece is in the right column
		        	if (gamePiece.transform.position.x == whichColumn)
		        	{
		        		// animate it falling and delete it
		        		StartCoroutine(dropPieceOut(gamePiece));

		        	}
			    }
			}

			if(gameObjectTurn == null)
			{
				gameObjectTurn = SpawnPiece();
			}

			if (!player1.keyRead){
				int column = player1.Key[1] - '0' - 1;
				if(int.TryParse(player1.Key, out column)){
					StartCoroutine(dropPiece(gameObjectTurn, column));
					howLongSinceDropOut++;
				}

				player1.keyRead = true;
			}


			/*
			if(gameObjectTurn == null)
			{
				gameObjectTurn = SpawnPiece();
				howLongSinceDropOut++;
				Debug.Log(howLongSinceDropOut);
			}
			else
			{
				// update the objects position
				Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				gameObjectTurn.transform.position = new Vector3(
					Mathf.Clamp(pos.x, 0, numColumns-1), 
					gameObjectField.transform.position.y + 1, 0);

				// click the left mouse button to drop the piece into the selected column
				if(Input.GetMouseButtonDown(0) && !mouseButtonPressed && !isDropping)
				{
					mouseButtonPressed= true;

					StartCoroutine(dropPiece(gameObjectTurn));
				}
				else
				{
					mouseButtonPressed = false;
				}
			}*/

			
			
			
		}

		/// <summary>
		/// Gets all the possible moves.
		/// </summary>
		/// <returns>The possible moves.</returns>
		public List<int> GetPossibleMoves()
		{
			List<int> possibleMoves = new List<int>();
			for (int x = 0; x < numColumns; x++)
			{
				for(int y = numRows - 1; y >= 0; y--)
				{
					if(field[x, y] == (int)Piece.Empty)
					{
						possibleMoves.Add(x);
						break;
					}
				}
			}
			return possibleMoves;
		}


		/// <summary>
		/// This method searches for a empty cell and lets 
		/// the object fall down into this cell
		/// </summary>
		/// <param name="gObject">Game Object.</param>
		IEnumerator dropPieceOut(GameObject gamePiece)
		{
			isDropping = true;

			Vector3 startPosition = gamePiece.transform.position;
			Vector3 endPosition = new Vector3(startPosition.x, -10, startPosition.z);

			float distance = Vector3.Distance(startPosition, endPosition);

			float t = 0;
			while(t < 1)
			{
				t += 0.03f;

				gamePiece.transform.position = Vector3.Lerp (startPosition, endPosition, t);
				yield return null;
			}

			// delete it
		    DestroyImmediate(gamePiece);

		    isDropping = false;
			yield return 0;
		}







		/// <summary>
		/// This method searches for a empty cell and lets 
		/// the object fall down into this cell
		/// </summary>
		/// <param name="gObject">Game Object.</param>
		IEnumerator dropPiece(GameObject gObject, int column)
		{
			isDropping = true;

			Vector3 startPosition = gObject.transform.position;
			Vector3 endPosition = new Vector3();

			// round to a grid cell
			startPosition = new Vector3(column, startPosition.y, startPosition.z);

			// is there a free cell in the selected column?
			bool foundFreeCell = false;
			for(int i = numRows-1; i >= 0; i--)
			{
				if(field[column, i] == 0)
				{
					foundFreeCell = true;
					
					// player 1 = yellow, player 0 = red
					field[column, i] = whichPlayersTurn ? (int)Piece.Yellow : (int)Piece.Red;
					
					endPosition = new Vector3(column, i * -1, startPosition.z);

					break;
				}
			}

			if(foundFreeCell)
			{
				// Instantiate a new Piece, disable the temporary
				GameObject g = Instantiate (gObject) as GameObject;
				gameObjectTurn.GetComponent<Renderer>().enabled = false;

				float distance = Vector3.Distance(startPosition, endPosition);

				float t = 0;
				while(t < 1)
				{
					t += Time.deltaTime * dropTime * ((numRows - distance) + 1);

					g.transform.position = Vector3.Lerp (startPosition, endPosition, t);
					yield return null;
				}

				g.transform.parent = gameObjectField.transform;

				// remove the temporary gameobject
				DestroyImmediate(gameObjectTurn);

				// run coroutine to check if someone has won
				StartCoroutine(Won());

				// wait until winning check is done
				while(isCheckingForWinner)
					yield return null;

				whichPlayersTurn = !whichPlayersTurn;
			}

			isDropping = false;

			yield return 0;
		}

		/// <summary>
		/// Check for Winner
		/// </summary>
		IEnumerator Won()
		{
			isCheckingForWinner = true;

			for(int x = 0; x < numColumns; x++)
			{
				for(int y = 0; y < numRows; y++)
				{
					// Get the Laymask to Raycast against, if its Players turn only include
					// Layermask Yellow otherwise Layermask Red
					int layermask = whichPlayersTurn ? (1 << 8) : (1 << 9);

					// If its Players turn ignore red as Starting piece and wise versa
					if(field[x, y] != (whichPlayersTurn ? (int)Piece.Yellow : (int)Piece.Red))
					{
						continue;
					}

					// shoot a ray of length 'numPiecesToWin - 1' to the right to test horizontally
					RaycastHit[] hitsHorz = Physics.RaycastAll(
						new Vector3(x, y * -1, 0), 
						Vector3.right, 
						numPiecesToWin - 1, 
						layermask);

					// return true (won) if enough hits
					if(hitsHorz.Length == numPiecesToWin - 1)
					{
						gameOver = true;
						break;
					}

					// shoot a ray up to test vertically
					RaycastHit[] hitsVert = Physics.RaycastAll(
						new Vector3(x, y * -1, 0), 
						Vector3.up, 
						numPiecesToWin - 1, 
						layermask);
					
					if(hitsVert.Length == numPiecesToWin - 1)
					{
						gameOver = true;
						break;
					}

					// test diagonally
					if(allowDiagonally)
					{
						// calculate the length of the ray to shoot diagonally
						float length = Vector2.Distance(new Vector2(0, 0), new Vector2(numPiecesToWin - 1, numPiecesToWin - 1));

						RaycastHit[] hitsDiaLeft = Physics.RaycastAll(
							new Vector3(x, y * -1, 0), 
							new Vector3(-1 , 1), 
							length, 
							layermask);
						
						if(hitsDiaLeft.Length == numPiecesToWin - 1)
						{
							gameOver = true;
							break;
						}

						RaycastHit[] hitsDiaRight = Physics.RaycastAll(
							new Vector3(x, y * -1, 0), 
							new Vector3(1 , 1), 
							length, 
							layermask);
						
						if(hitsDiaRight.Length == numPiecesToWin - 1)
						{
							gameOver = true;
							break;
						}
					}

					yield return null;
				}

				yield return null;
			}

			// if Game Over update the winning text to show who has won
			if(gameOver == true)
			{
				winningText.GetComponent<TextMesh>().text = whichPlayersTurn ? yellowPlayerWinText : redPlayerWinText;
			}
			else 
			{
				// check if there are any empty cells left, if not set game over and update text to show a draw
				if(!FieldContainsEmptyCell())
				{
					gameOver = true;
					winningText.GetComponent<TextMesh>().text = drawText;
				}
			}

			isCheckingForWinner = false;

			yield return 0;
		}

		/// <summary>
		/// check if the field contains an empty cell
		/// </summary>
		/// <returns><c>true</c>, if it contains empty cell, <c>false</c> otherwise.</returns>
		bool FieldContainsEmptyCell()
		{
			for(int x = 0; x < numColumns; x++)
			{
				for(int y = 0; y < numRows; y++)
				{
					if(field[x, y] == (int)Piece.Empty)
						return true;
				}
			}
			return false;
		}
	}
}
