import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MemberService } from '../../../core/services/member-service';
import { Observable } from 'rxjs';
import { Photo } from '../../../types/member';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-member-photos',
  imports: [AsyncPipe],
  templateUrl: './member-photos.html',
  styleUrl: './member-photos.css'
})
export class MemberPhotos {
  private memberService = inject(MemberService);
  private route = inject(ActivatedRoute);
  protected photos$: Observable<Photo[]>;

  constructor() {
    const memberId = this.route.parent?.snapshot.paramMap.get('id');
    if (memberId) {
      this.photos$ = this.memberService.getMemmberPhotos(memberId);
    } else {
      this.photos$ = new Observable<Photo[]>(subscriber => {
        subscriber.error('Member ID not found');
      });
    } 
  }

  get photoMocks() {
    return Array.from({ length: 20}, (_,i) => ({
      url: '/user.png'
    }))
  }
}
