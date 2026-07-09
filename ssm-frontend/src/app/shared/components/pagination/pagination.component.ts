import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  templateUrl: './pagination.component.html',
  styleUrl: './pagination.component.css'
})
export class PaginationComponent {
  pageNumber = input.required<number>();
  totalPages = input.required<number>();
  hasNext = input.required<boolean>();
  hasPrevious = input.required<boolean>();
  pageChange = output<number>();
}
