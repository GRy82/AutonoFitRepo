function checkCardio(goalOneValue, goalTwoValue) {
    if (goalOneValue == 4 || goalOneValue == 5 || goalTwoValue == 4 || goalTwoValue == 5) {
        document.getElementById("mile-min").style.display = "inline";
        document.getElementById("mile-sec").style.display = "inline-block";
    }
    else {
        document.getElementById("mile-min").style.display = "none";
        document.getElementById("mile-sec").style.display = "none";
    }
    if ((goalOneValue == 1 || goalOneValue == 2 || goalTwoValue == 1 || goalTwoValue == 2) && alertAlreadyActivated === false) {
        alert("Because you do not own equipment conducive to heavier lifting exercises, it is unlikely that the workout generated will"
            + " be suitable towards your goals if those goals include strength or hypertrophy.");

        alertAlreadyActivated = true;
    }
    
}