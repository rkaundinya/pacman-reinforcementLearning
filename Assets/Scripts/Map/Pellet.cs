using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Pellet : MonoBehaviour
{
    public int points = 10;
    private bool eaten = false;

    protected virtual void Eat()
    {
        GameManager.gm.PelletEaten(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!eaten && other.gameObject.layer == LayerMask.NameToLayer("Pacman")) {
            Eat();
            eaten = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Update bitmap with ghost code
        if (other.gameObject.layer == LayerMask.NameToLayer("Ghost"))
        {
            // Check if ghost is frightened or not and update accordingly

            // Check if ghost is eaten or not and update accodingly

        }
        // Update bitmap with pacman code

    }

    public void Reset()
    {
        eaten = false;
    }

}
