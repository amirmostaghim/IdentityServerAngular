import { LoginInputModel } from "./../models/loginViewModels/LoginInputModel.model";
import { HttpClientService } from "./../services/http-client.service";
import {
  Component,
  OnInit,
  ViewEncapsulation,
  Output,
  EventEmitter,
  ElementRef,
  OnDestroy,
  Renderer2
} from "@angular/core";
import {
  FormControl,
  FormGroup,
  FormBuilder,
  Validators
} from "@angular/forms";
import { ActivatedRoute } from "@angular/router";

@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.css"],
  encapsulation: ViewEncapsulation.Emulated
})
export class LoginComponent implements OnInit, OnDestroy {
  @Output() emitter = new EventEmitter<any>();
  loginForm: FormGroup;

  constructor(
    private formBuilder: FormBuilder,
    private elRef: ElementRef,
    private renderer: Renderer2,
    private httpClient: HttpClientService,
    private activatedRoute: ActivatedRoute
  ) {}

  ngOnInit() {
    this.renderer.addClass(
      this.elRef.nativeElement.ownerDocument.body,
      "bg-login-style"
    );
    // this.loginForm = new FormGroup({
    //   userName: new FormControl(),
    //   password: new FormControl()
    // });

    this.loginForm = this.formBuilder.group({
      userName: ["", Validators.required],
      password: ["", Validators.required]
    });
  }

  ngOnDestroy(): void {
    this.renderer.removeClass(
      this.elRef.nativeElement.ownerDocument.body,
      "bg-login-style"
    );
  }
  onSubmit() {
console.log(this.activatedRoute.fragment);

    const formValue = this.loginForm.value;
    const loginInputModel = new LoginInputModel(formValue.userName, formValue.password, formValue.rememberLogin, formValue.returnUrl);
    this.httpClient
      .post<LoginInputModel>(loginInputModel)
      .subscribe((result: LoginInputModel) => {

      });
    this.emitter.emit(this.loginForm.valid);
  }
}
