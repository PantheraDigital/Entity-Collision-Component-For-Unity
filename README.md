# Entity-Collision-Component-For-Unity
C# class for use in Unity2D made to handle collision detection for a character controller.


> Compatible with CapsuleCollider2D, BoxCollider2D, CircleCollider2D.
> Collision Component checks take into account the size, offset, and rotation of collider when running checks.

> Uses Physics2D.OverlapBoxAll or Physics2D.OverlapCircleAll to check around the collider of the object. 
> Uses Physics2D.Raycast to cast 3 rays along the bottom of the collider to get detailed info about the collider the object is on. The RaycastHit2D can be retrieved from each of the three rays as well as the slope from a specific ray or the average slope of all the rays that have hit something. 
> Contains functionality to ignore all colliders on a layer within a range of object. This will allow the object to pass through specific objects. Will also re-enable collision with those objects once out of range.
