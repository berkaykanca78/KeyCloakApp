import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

const API_BASE = 'https://localhost:5001';

export interface CityDto {
  id: number;
  name: string;
  plateCode: number;
}

export interface DistrictDto {
  id: number;
  name: string;
  cityId: number;
}

@Injectable({ providedIn: 'root' })
export class CitiesApi {
  private readonly http = inject(HttpClient);

  getCities() {
    return this.http.get<CityDto[]>(`${API_BASE}/api/cities`);
  }

  getDistricts(cityId: number) {
    return this.http.get<DistrictDto[]>(`${API_BASE}/api/cities/${cityId}/districts`);
  }
}
