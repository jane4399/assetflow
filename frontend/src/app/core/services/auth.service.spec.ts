import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { AuthResponse } from '../models/auth';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  const sampleResponse: AuthResponse = {
    accessToken: 'jwt-token-123',
    expiresAtUtc: new Date().toISOString(),
    user: { id: '11111111-1111-1111-1111-111111111111', email: 'admin@assetflow.io', fullName: 'Admin', role: 'Admin' }
  };

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('is created', () => {
    expect(service).toBeTruthy();
  });

  it('POSTs credentials and persists the token/user on login', () => {
    let received: AuthResponse | undefined;
    service.login({ email: 'admin@assetflow.io', password: 'Admin123!' }).subscribe((r) => (received = r));

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush(sampleResponse);

    expect(received).toEqual(sampleResponse);
    expect(service.token).toBe('jwt-token-123');
    expect(service.isAuthenticated()).toBeTrue();
    expect(service.isAdmin()).toBeTrue();
    expect(service.user()?.email).toBe('admin@assetflow.io');
  });

  it('clears the token and user on logout', () => {
    service.login({ email: 'admin@assetflow.io', password: 'Admin123!' }).subscribe();
    httpMock.expectOne(`${environment.apiBaseUrl}/auth/login`).flush(sampleResponse);

    service.logout();

    expect(service.token).toBeNull();
    expect(service.isAuthenticated()).toBeFalse();
    expect(service.user()).toBeNull();
  });
});
