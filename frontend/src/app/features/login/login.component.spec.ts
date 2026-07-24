import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { LoginComponent } from './login.component';

const activatedRouteStub = {
  snapshot: { queryParamMap: convertToParamMap({}) }
} as unknown as ActivatedRoute;

describe('LoginComponent', () => {
  let fixture: ComponentFixture<LoginComponent>;
  let component: LoginComponent;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: ActivatedRoute, useValue: activatedRouteStub }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('is created', () => {
    expect(component).toBeTruthy();
  });

  it('is valid with the seeded demo credentials prefilled', () => {
    expect(component.form.valid).toBeTrue();
  });

  it('becomes invalid when the email is cleared', () => {
    component.form.controls.email.setValue('');
    expect(component.form.valid).toBeFalse();
  });

  it('becomes invalid with a malformed email', () => {
    component.form.controls.email.setValue('not-an-email');
    expect(component.form.controls.email.valid).toBeFalse();
  });
});
