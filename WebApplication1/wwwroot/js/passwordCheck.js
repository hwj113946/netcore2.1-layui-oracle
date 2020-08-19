function Check(password) {
    var one = "^(?=.*[a-zA-Z])(?=.*[0-9])(?=.*[._~!@#$^&*])[A-Za-z0-9._~!@#$^&*]{8,20}$";
    if (password.match(one)) {
        return true;
    } else {
        return false;
    }
}