using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CarMovement : MonoBehaviour
{
	[SerializeField]
	private Tilemap ground;
	[SerializeField]
	private Tilemap fogOfWar;
	[SerializeField]
	private Sprite[] spriteOrientations;

	private PlayerInput playerInput;
	private Vector2 direction;
	private string startingOrientation = "up";
	private string previousDirection = "";
	private bool isMoving = false;
	private bool checkCurve = false;
	private CarAnimation carAnimation;

	private void Awake() {
		carAnimation = GetComponent<CarAnimation>();

		playerInput = new PlayerInput();
		playerInput.Car.Movement.performed += ctx => {
			direction = ctx.ReadValue<Vector2>();
			CanMove();
		};
	}

	private void OnEnable() {
		playerInput.Enable();
	}

	private void OnDisable() {
		playerInput.Disable();
	}

	private void Update() {
		InCurveBehaviour();
	}

	private void Move(Vector3Int nextGridPosition) {
		isMoving = true;
		fogOfWar.SetTile(nextGridPosition, null);

		if (direction.x == 0) {
			if (direction.y == 1) {
				previousDirection = "up";
				if (carAnimation != null)
					carAnimation.Animate(startingOrientation, previousDirection, direction);
			}
			else {
				previousDirection = "down";
				if (carAnimation != null)
					carAnimation.Animate(startingOrientation, previousDirection, direction);
			}
		}
		else if (direction.x == 1) {
			previousDirection = "right";
			if (carAnimation != null)
				carAnimation.Animate(startingOrientation, previousDirection, direction);
		}
		else {
			previousDirection = "left";
			if (carAnimation != null)
				carAnimation.Animate(startingOrientation, previousDirection, direction);
		}

		if (carAnimation == null) {
			transform.position += (Vector3)direction;
			isMoving = false;
		}
	}

	private void CanMove() {
		if (isMoving)
			return;

		Vector3Int actualGridPosition = ground.WorldToCell(transform.position);
		Vector3Int nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);

		// Check if the direction the player want to move is valid based on the tile he's on
		switch (ground.GetTile(actualGridPosition).name) {
			case "roadNEWS":
				// Here the player can move in any direction
				if (isCellNotEmpty(nextGridPosition))
					Move(nextGridPosition);
				break;

			case "roadEW":
				// Check if the player moved horizontally
				if (direction.y == 0) {
					if (isCellNotEmpty(nextGridPosition))
						Move(nextGridPosition);
				}
				break;

			case "roadNS":
				// Check if the player moved vertically
				if (direction.x == 0) {
					if (isCellNotEmpty(nextGridPosition))
						Move(nextGridPosition);
				}
				break;
		}

		// This is nedeed so we can check in the Update if the player has landed in a curve
		checkCurve = true;
	}

	private bool isCellNotEmpty(Vector3Int nextGridPosition) {
		// Check if exist a tile in the ground tilemap in the direction the player want to move
		if (ground.HasTile(nextGridPosition) && direction.magnitude <= 1) // The second check is nedeed so the player can't move diagonally
			return true;
		else
			return false;
	}

	/*
	 * Mangage the player position if he is in a curve (tunnel) after he moved
	 * The player is moved in another cell based on the direction from which it comes
	 * When it will no longer be in a curve the loop will end
	 */
	public void InCurveBehaviour() {
		if (checkCurve && !isMoving) {
			Vector3Int actualGridPosition;
			Vector3Int nextGridPosition;
			actualGridPosition = ground.WorldToCell(transform.position);
			switch (ground.GetTile(actualGridPosition).name) {
				case "roadNE":
					if (previousDirection == "down") {
						direction = new Vector2(1, 0);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "right";
					}
					else {
						direction = new Vector2(0, 1);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "up";
					}
					break;

				case "roadNW":
					if (previousDirection == "down") {
						direction = new Vector2(-1, 0);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "left";
					}
					else {
						direction = new Vector2(0, 1);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "up";
					}
					break;

				case "roadSE":
					if (previousDirection == "up") {
						direction = new Vector2(1, 0);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "right";
					}
					else {
						direction = new Vector2(0, -1);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "down";
					}
					break;

				case "roadSW":
					if (previousDirection == "up") {
						direction = new Vector2(-1, 0);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "left";
					}
					else {
						direction = new Vector2(0, -1);
						nextGridPosition = ground.WorldToCell(transform.position + (Vector3)direction);
						Move(nextGridPosition);
						previousDirection = "down";
					}
					break;
			}

			if (ground.GetTile(actualGridPosition).name != "roadNE" &&
				ground.GetTile(actualGridPosition).name != "roadNW" &&
				ground.GetTile(actualGridPosition).name != "roadSE" &&
				ground.GetTile(actualGridPosition).name != "roadSW")
				checkCurve = false;
		}
	}

	public void AnimationFinished() {
		startingOrientation = previousDirection;
		isMoving = false;
	}
}
