import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

@Injectable({
  providedIn: "root"
})
export class HttpClientService {

  baseUrl = 'https://localhost:6001';

  constructor(private httpClient: HttpClient) {}

  get<T>(): Observable<T> {
    return this.httpClient.get<T>(this.baseUrl);
  }

  post<T>(body: T): Observable<T> {
    return this.httpClient.post<T>(this.baseUrl, body);
  }
}
