import { CategoryType } from '../../enums/transaction-type.enum';

export interface CategoryCreateRequest {
    name: string;
    type: CategoryType;
    color: string;
}
