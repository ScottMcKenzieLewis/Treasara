import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TrsInfoTipComponent } from './trs-info-tip.component';

describe('TrsInfoTipComponent', () => {
  let component: TrsInfoTipComponent;
  let fixture: ComponentFixture<TrsInfoTipComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TrsInfoTipComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TrsInfoTipComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
