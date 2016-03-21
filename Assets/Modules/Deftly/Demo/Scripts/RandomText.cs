// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
using UnityEngine.UI;

public class RandomText : MonoBehaviour
{
    public Text UpperText;
    public Text LowerText;

	void Start ()
	{
	    string wordsUpper = "";
        string wordsLower = "";
	    int rng1 = Random.Range(0, 5);
        int rng2 = Random.Range(0, 5);
	    switch (rng1)
	    {
	        case 0:
	            wordsUpper = "you are dead.";
	            break;
            case 1:
	            wordsUpper = "you area leaking red fluids.";
	            break;
            case 2:
	            wordsUpper = "you have fallen on the ground.";
	            break;
            case 3:
	            wordsUpper = "are you taking a nap?";
	            break;
            case 4:
	            wordsUpper = "you are broken.";
	            break;
            case 5:
	            wordsUpper = "there is no cow level.";
	            break;
	    }

        switch (rng2)
        {
            case 0:
                wordsLower = "how unfortunate.";
                break;
            case 1:
                wordsLower = "we are not amused.";
                break;
            case 2:
                wordsLower = "perhaps lower the difficult level.";
                break;
            case 3:
                wordsLower = "interesting.";
                break;
            case 4:
                wordsLower = "dysentery strikes again.";
                break;
            case 5:
                wordsLower = "your family would be ashamed.";
                break;
        }

	    UpperText.text = wordsUpper;
	    LowerText.text = wordsLower;
	}
}
