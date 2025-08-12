import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { EditableMember, Member, MemberParams, Photo } from '../../types/member';
import { AccountService } from './account-service';
import { tap } from 'rxjs';
import { PaginatedResult } from '../../types/pagination';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  private baseUrl = environment.apiUrl;
  editMode = signal<boolean>(false);
  member = signal<Member | null>(null);

  getMembers(MemberParams: MemberParams) {
    let params = new HttpParams();
    params = params.append('pageNumber', MemberParams.pageNumber);
    params = params.append('pageSize', MemberParams.pageSize);
    params = params.append('minAge', MemberParams.minAge);
    params = params.append('maxAge', MemberParams.maxAge);
    params = params.append('orderBy', MemberParams.orderBy);

    if (MemberParams.gender) {
      params = params.append('gender', MemberParams.gender);
    }


    return this.http.get<PaginatedResult<Member>>(this.baseUrl + 'members', {params}).pipe(
      tap(() => {
        // Cache the members data in the service
        localStorage.setItem('filters', JSON.stringify(MemberParams));
      })  
    );
  }

  getMember(id: string) {
    return this.http.get<Member>(this.baseUrl + 'members/' + id).pipe(
      // Cache the member data in the service
      tap(member => {
        this.member.set(member);
      })
    );
  }

  getMemmberPhotos(id: string) {
    return this.http.get<Photo[]>(this.baseUrl + 'members/' + id + '/photos');
  }

  updateMember(member: EditableMember) {
    return this.http.put(this.baseUrl + 'members/', member);
  }

  uploadPhoto(file: File) {
    const formData = new FormData();
    formData.append('file',file);
    return this.http.post<Photo>(this.baseUrl + 'members/add-photo', formData);
  }

  setMainPhoto(photo: Photo){
    return this.http.put(this.baseUrl + 'members/set-main-photo/'+photo.id, {});
  }

  deletePhoto(photoId: number){
    return this.http.delete(this.baseUrl + 'members/delete-photo/' + photoId);
  }
}
