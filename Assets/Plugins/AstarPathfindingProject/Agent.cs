using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class Agent : MonoBehaviour {

    public float speed { get; set; }
    public float acceleration { get; set; }
    public float angularSpeed { get; set; }
    public float remainingDistance { get { return GetDistanceToTarget(); } }
    public float stoppingDistance { get; set; }
    public bool isPathStale
    {
        get { if (path != null) return path.error; else return false; }
    }
    public Vector3 destination { get; set; }
    public Vector3 desiredVelocity { get { return CalculateVelocity(transform.position); } }

    /** Determines how often it will search for new paths.
	 * If you have fast moving targets or AIs, you might want to set it to a lower value.
	 * The value is in seconds between path requests.
	 */
    public float repathRate = 0.5F;

    /** Enables or disables searching for paths.
	 * Setting this to false does not stop any active path requests from being calculated or stop it from continuing to follow the current path.
	 * \see #canMove
	 */
    public bool canSearch = true;

    /** Enables or disables movement.
	  * \see #canSearch */
    public bool canMove = true;

    /** Distance from the destination point where the AI will start to slow down.
	 * Note that this doesn't only affect the end point of the path
 	 * but also any intermediate points, so be sure to set #forwardLook and #pickNextWaypointDist to a higher value than this
 	 */
    public float slowdownDistance = 1.0F;

    /** Determines within what range it will switch to destination the next waypoint in the path */
    public float pickNextWaypointDist = 0.1f;

    /** destination point is Interpolated on the current segment in the path so that it has a distance of #forwardLook from the AI.
	  * See the detailed description of AIPath for an illustrative image */
    public float forwardLook = 1;

    /** Do a closest point on path check when receiving path callback.
	 * Usually the AI has moved a bit between requesting the path, and getting it back, and there is usually a small gap between the AI
	 * and the closest node.
	 * If this option is enabled, it will simulate, when the path callback is received, movement between the closest node and the current
	 * AI position. This helps to reduce the moments when the AI just get a new path back, and thinks it ought to move backwards to the start of the new path
	 * even though it really should just proceed forward.
	 */
    public bool closestOnPathCheck = true;

    private float minMoveScale = 0.05F;   
    private Seeker seeker;    
    private Transform tr;    
    private float lastRepath = -9999;
    private Path path;
    private Rigidbody2D rigid;
    private int currentWaypointIndex = 0;
    private bool targetReached = false;
    private bool canSearchAgain = true;
    private Vector3 lastFoundWaypointPosition;
    private float lastFoundWaypointTime = -9999;
    public bool TargetReached { get { return targetReached; } }
    private bool startHasRun = false;

    private void Awake()
    {
        seeker = GetComponent<Seeker>();

        //This is a simple optimization, cache the transform component lookup
        tr = transform;

        //Cache some other components (not all are necessarily there)
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Stop()
    {
        canMove = false;
        canSearch = false;
    }

    public void Resume()
    {
        canMove = true;
        canSearch = true;
    }

    public void Start()
    {
        startHasRun = true;
        OnEnable();
    }

    public void OnEnable()
    {

        lastRepath = -9999;
        canSearchAgain = true;

        lastFoundWaypointPosition = tr.position;

        if (startHasRun)
        {
            //Make sure we receive callbacks when paths complete
            seeker.pathCallback += OnPathComplete;

            StartCoroutine(RepeatTrySearchPath());
        }
    }

    public void OnDisable()
    {
        // Abort calculation of path
        if (seeker != null && !seeker.IsDone()) seeker.GetCurrentPath().Error();

        // Release current path
        if (path != null) path.Release(this);
        path = null;

        //Make sure we receive callbacks when paths complete
        seeker.pathCallback -= OnPathComplete;
    }

    /** Tries to search for a path every #repathRate seconds.
	  * \see TrySearchPath
	  */
    private IEnumerator RepeatTrySearchPath()
    {
        while (true)
        {
            float v = TrySearchPath();
            yield return new WaitForSeconds(v);
        }
    }

    /** Tries to search for a path.
	 * Will search for a new path if there was a sufficient time since the last repath and both
	 * #canSearchAgain and #canSearch are true and there is a destination.
	 *
	 * \returns The time to wait until calling this function again (based on #repathRate)
	 */
    public float TrySearchPath()
    {
        if (Time.time - lastRepath >= repathRate && canSearchAgain && canSearch)
        {
            SearchPath();
            return repathRate;
        }
        else
        {
            //StartCoroutine (WaitForRepath ());
            float v = repathRate - (Time.time - lastRepath);
            return v < 0 ? 0 : v;
        }
    }

    /** Requests a path to the destination */
    public void SearchPath()
    {        
        lastRepath = Time.time;
        //This is where we should search to

        canSearchAgain = false;

        //Alternative way of requesting the path
        ABPath p = ABPath.Construct (tr.position,destination,null);
        seeker.StartPath (p);

        //We should search from the current position
        //seeker.StartPath(tr.position, destination);
    }

    public void OnTargetReached()
    {
        //End of path has been reached
        //If you want custom logic for when the AI has reached it's destination
        //add it here
        //You can also create a new script which inherits from this one
        //and override the function in that script
    }

    /** Called when a requested path has finished calculation.
	  * A path is first requested by #SearchPath, it is then calculated, probably in the same or the next frame.
	  * Finally it is returned to the seeker which forwards it to this function.\n
	  */
    public void OnPathComplete(Path _p)
    {
        ABPath p = _p as ABPath;
        if (p == null) throw new System.Exception("This function only handles ABPaths, do not use special path types");

        canSearchAgain = true;

        //Claim the new path
        p.Claim(this);

        // Path couldn't be calculated of some reason.
        // More info in p.errorLog (debug string)
        if (p.error)
        {
            p.Release(this);
            return;
        }

        //Release the previous path
        if (path != null) path.Release(this);

        //Replace the old path
        path = p;

        //Reset some variables
        currentWaypointIndex = 0;
        targetReached = false;

        //The next row can be used to find out if the path could be found or not
        //If it couldn't (error == true), then a message has probably been logged to the console
        //however it can also be got using p.errorLog
        //if (p.error)

        if (closestOnPathCheck)
        {
            Vector3 p1 = Time.time - lastFoundWaypointTime < 0.3f ? lastFoundWaypointPosition : p.originalStartPoint;
            Vector3 p2 = tr.position;
            Vector3 dir = p2 - p1;
            float magn = dir.magnitude;
            dir /= magn;
            int steps = (int)(magn / pickNextWaypointDist);


            for (int i = 0; i <= steps; i++)
            {
                CalculateVelocity(p1);
                p1 += dir;
            }

        }
    }

    public  void Update()
    {

        if (!canMove) { return; }

        Vector3 dir = CalculateVelocity(tr.position);

        RotateTowards(destinationDirection);

        if (rigid != null)
        {
            rigid.MovePosition(tr.position + dir);
        }
    }

    /** Point to where the AI is heading.
	  * Filled in by #CalculateVelocity */
    private Vector3 destinationPoint;
    /** Relative direction to where the AI is heading.
	 * Filled in by #CalculateVelocity */
    private Vector3 destinationDirection;

    private float XYSqrMagnitude(Vector3 a, Vector3 b)
    {
        float dx = b.x - a.x;
        float dy = b.y - a.y;
        return dx * dx + dy * dy;
    }

    /** Calculates desired velocity.
	 * Finds the destination path segment and returns the forward direction, scaled with speed.
	 * A whole bunch of restrictions on the velocity is applied to make sure it doesn't overshoot, does not look too far ahead,
	 * and slows down when close to the destination.
	 * /see speed
	 * /see stoppingDistance
	 * /see slowdownDistance
	 * /see CalculateTargetPoint
	 * /see targetPoint
	 * /see targetDirection
	 * /see currentWaypointIndex
	 */
    private Vector3 CalculateVelocity(Vector3 currentPosition)
    {
        if (path == null || path.vectorPath == null || path.vectorPath.Count == 0) return Vector3.zero;

        List<Vector3> vPath = path.vectorPath;

        if (vPath.Count == 1)
        {
            vPath.Insert(0, currentPosition);
        }

        if (currentWaypointIndex >= vPath.Count) { currentWaypointIndex = vPath.Count - 1; }

        if (currentWaypointIndex <= 1) currentWaypointIndex = 1;

        while (true)
        {
            if (currentWaypointIndex < vPath.Count - 1)
            {
                //There is a "next path segment"
                float dist = XYSqrMagnitude(vPath[currentWaypointIndex], currentPosition);
                //Mathfx.DistancePointSegmentStrict (vPath[currentWaypointIndex+1],vPath[currentWaypointIndex+2],currentPosition);
                if (dist < pickNextWaypointDist * pickNextWaypointDist)
                {
                    lastFoundWaypointPosition = currentPosition;
                    lastFoundWaypointTime = Time.time;
                    currentWaypointIndex++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        Vector3 dir = vPath[currentWaypointIndex] - vPath[currentWaypointIndex - 1];
        Vector3 destinationPosition = CalculateDestinationPoint(currentPosition, vPath[currentWaypointIndex - 1], vPath[currentWaypointIndex]);


        dir = destinationPosition - currentPosition;
        dir.z = 0;
        float targetDist = dir.magnitude;

        float slowdown = Mathf.Clamp01(targetDist / slowdownDistance);

        this.destinationDirection = dir;
        this.destinationPoint = destinationPosition;

        if (currentWaypointIndex == vPath.Count - 1 && targetDist <= stoppingDistance)
        {
            if (!targetReached) { targetReached = true; OnTargetReached(); }

            //Send a move request, this ensures gravity is applied
            return Vector3.zero;
        }

        Vector3 right = tr.right;
        float dot = Vector3.Dot(dir.normalized, right);
        float sp = speed * Mathf.Max(dot, minMoveScale) * slowdown;


        if (Time.deltaTime > 0)
        {
            sp = Mathf.Clamp(sp, 0, targetDist / (Time.deltaTime * 2));
        }
        return right * sp;
    }

    /** Calculates destination point from the current line segment.
	 * \param p Current position
	 * \param a Line segment start
	 * \param b Line segment end
	 * The returned point will lie somewhere on the line segment.
	 * \see #forwardLook
	 * \todo This function uses .magnitude quite a lot, can it be optimized?
	 */
    private Vector3 CalculateDestinationPoint(Vector3 p, Vector3 a, Vector3 b)
    {
        a.z = p.z;
        b.z = p.z;

        float magn = (a - b).magnitude;
        if (magn == 0) return a;

        float closest = AstarMath.Clamp01(AstarMath.NearestPointFactor(a, b, p));
        Vector3 point = (b - a) * closest + a;
        float distance = (point - p).magnitude;

        float lookAhead = Mathf.Clamp(forwardLook - distance, 0.0F, forwardLook);

        float offset = lookAhead / magn;
        offset = Mathf.Clamp(offset + closest, 0.0F, 1.0F);
        return (b - a) * offset + a;
    }

    public void RotateTowards(Vector3 dir)
    {
        if (dir == Vector3.zero) return;

        Quaternion rot = tr.rotation;

        float AngleRad = Mathf.Atan2(dir.y, dir.x);
        float AngleDeg = Mathf.Rad2Deg * AngleRad;
        Quaternion fin = Quaternion.Euler(0, 0, AngleDeg);
        tr.rotation = Quaternion.Slerp(rot, fin, Time.deltaTime * angularSpeed);
    }

    private float GetDistanceToTarget()
    {
        if (path == null || path.vectorPath == null || path.vectorPath.Count == 0) return 0;

        List<Vector3> vPath = path.vectorPath;
        float totalDistance = 0;

        Vector3 current = transform.position;

        //Iterate through vPath and find the distance between the nodes
        for (int i = currentWaypointIndex; i < vPath.Count; i++)
        {
            totalDistance += (vPath[i] - current).magnitude;
            current = vPath[i];
        }

        totalDistance += (destinationPoint - current).magnitude;

        return totalDistance;
    }
}
