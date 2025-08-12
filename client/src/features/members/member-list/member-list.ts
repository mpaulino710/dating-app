import { Component, inject, OnInit, signal, ViewChild } from '@angular/core';
import { MemberService } from '../../../core/services/member-service';
import { Observable } from 'rxjs';
import { Member, MemberParams } from '../../../types/member';
import { AsyncPipe } from '@angular/common';
import { MemberCard } from "../member-card/member-card";
import { PaginatedResult } from '../../../types/pagination';
import { Paginator } from "../../../shared/paginator/paginator";
import { FilterModal } from '../../../feature/members/filter-modal/filter-modal';

@Component({
  selector: 'app-member-list',
  imports: [MemberCard, Paginator, FilterModal],
  templateUrl: './member-list.html',
  styleUrl: './member-list.css'
})
export class MemberList implements OnInit {
  @ViewChild('FilterModal') modal!: FilterModal
  private memberService = inject(MemberService);
  protected paginatedMembers = signal<PaginatedResult<Member> | null>(null);
  protected memberParams = new MemberParams();
  private updatedParams = new MemberParams();

 
  constructor() {
    const filter = localStorage.getItem('filters');
    if (filter) {
      this.memberParams = JSON.parse(filter);
      this.updatedParams = JSON.parse(filter);
    }
  }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers() {
    this.memberService.getMembers(this.memberParams).subscribe({
      next: result => {
        this.paginatedMembers.set(result);
      }
    });
  }

  onPageChage(event: { pageNumber: number; pageSize: number; }) {
    this.memberParams.pageSize = event.pageSize;
    this,this.memberParams.pageNumber = event.pageNumber;
    this.loadMembers();
  }

  openModal() {
    this.modal.open();
  }

  onCloseModal() {
    console.log('Modal closed');
  }

  onFilterChange(params: MemberParams) {
    this.memberParams = {...params};
    this.updatedParams = {...params};
    this.loadMembers();
  }

  resetFilters() {
    this.memberParams = new MemberParams();
    this.updatedParams = new MemberParams();
    this.loadMembers();
  }

  get displayMessage(): string {
    const defaultParams = new MemberParams();
    const filters: string[] = [];

    if (this.updatedParams.gender) {
      filters.push(this.updatedParams.gender + 's');
    } else {
      filters.push('Males, Females');
    }

    if (this.updatedParams.minAge !== defaultParams.minAge 
        || this.updatedParams.maxAge !== defaultParams.maxAge) {
      filters.push(` ages ${this.updatedParams.minAge} - ${this.updatedParams.maxAge}`);
    }

    filters.push(this.updatedParams.orderBy === 'lastActive' ? 'Recently active' : 'Newest members');

    return filters.length > 0 ? `Selected: ${filters.join('  | ')}` : 'All members';
  }
}
