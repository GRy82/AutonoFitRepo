function checkCardio(goalOneValue, goalTwoValue) {
    if (goalOneValue == 4 || goalOneValue == 5 || goalTwoValue == 4 || goalTwoValue == 5) {
        document.getElementById("mile-min").style.display = "inline";
        document.getElementById("mile-sec").style.display = "inline-block";
    }
    else {
        document.getElementById("mile-min").style.display = "none";
        document.getElementById("mile-sec").style.display = "none";
    }
}