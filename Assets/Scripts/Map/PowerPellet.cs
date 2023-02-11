using UnityEngine;

public class PowerPellet : Pellet
{
    public float duration = 8f;

    protected override void Eat()
    {
        activeBitmapCodes.Remove(BitmapCode.PowerPellet);
        GameManager.gm.PowerPelletEaten(this);
    }

    protected override void RegisterWithBitmap()
    {
        if(GameManager.gm.stateRepresentation.DebugCheckBitmapLoaction(gameObject.transform.position))
        {
            Debug.Log("Error - trying to re-register existing pellet location to bitmap");
        }

        activeBitmapCodes.Add(BitmapCode.PowerPellet, 1);
        GameManager.gm.stateRepresentation.AddToBitmap(gameObject.transform.position, BitmapCode.PowerPellet);
    }

}
