export class LoginInputModel {
  public userName: string;
  public password: string;
  public rememberLogin: boolean;
  public returnUrl: string;

  constructor(userName: string, password: string, rememberLogin: boolean, returnUrl: string ) {
    this.userName = userName;
    this.password = password;
    this.rememberLogin = rememberLogin;
    this.returnUrl = returnUrl;
  }
}
