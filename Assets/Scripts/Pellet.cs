using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Pellet : MonoBehaviour
{
    public int points = 10;
    private bool eaten = false;

    protected virtual void Eat()
    {
        FindObjectOfType<GameManager>().PelletEaten(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!eaten && other.gameObject.layer == LayerMask.NameToLayer("Pacman")) {
            Eat();
            eaten = true;
        }
    }

}
