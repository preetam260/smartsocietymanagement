export function generateDummyPaymentId(): string {
  return 'pay_' + Math.random().toString(36).substring(2, 15);
}


export const DUMMY_SIGNATURE = 'valid_sig';
