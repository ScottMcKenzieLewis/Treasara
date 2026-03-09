import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BondValuationPageComponent } from './bond-valuation-page.component';

describe('BondValuationPageComponent', () => {
  let component: BondValuationPageComponent;
  let fixture: ComponentFixture<BondValuationPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BondValuationPageComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(BondValuationPageComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
